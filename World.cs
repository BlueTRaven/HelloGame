using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Protobuf;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using HelloGame.Entities;
using HelloGame.Guis;
using HelloGame.Guis.Widgets;
using HelloGame.Utility;
using HelloGame.Hits;

using Humper;
using HelloGame.Entities.Particles;

namespace HelloGame
{
    public class World
    {
        public string name { get; private set; }
        public string displayName;
        public Color backgroundColor;

        public const int mode_brushes = 0, mode_walls = 1, mode_spawners = 2, mode_props = 3, mode_trigger = 4, mode_selection = 5;

        public Brush[] brushes;
        public Wall[] walls;
        public Prop[] props;
        public EntitySpawner[] spawners;
        public Trigger[] triggers;

        public List<IDamageTaker> damageTakers;

        public Player player;
        public Entity[] entities;

        public Hits.Hit[] hitboxes;
        public Humper.World collisionWorld;

        public List<Vector2> editorPoints;

        public List<ISelectable> selected;
        public ISelectable trueSelected;
        public int trueSelectedIndex;

        public Dictionary<int, Type> entityTypes;

        public World(string playerSaveName)
        {
            backgroundColor = Color.Black;

            Trigger.LoadTriggerActions();

            brushes = new Brush[128];
            walls = new Wall[128];
            spawners = new EntitySpawner[128];
            props = new Prop[128];
            triggers = new Trigger[128];

            entities = new Entity[128];

            hitboxes = new Hits.Hit[128];

            damageTakers = new List<IDamageTaker>();

            collisionWorld = new Humper.World(8192, 8192);

            player = new Player(collisionWorld.Create(0, 0, 32, 32));

            if (File.Exists("Saves/" + playerSaveName + ".hgsf"))
            {
                Player.Load(this, player, playerSaveName + ".hgsf");
            }
            else
            {
                Load("citadel1_1"); //We load here because player.Save requires us to know the save name, and there is no default.
                Player.Load(this, player.Save(0, name, playerSaveName));    //lol
            }

            editorPoints = new List<Vector2>(2);

            selected = new List<ISelectable>();

            entityTypes = new Dictionary<int, Type>();
        }

        #region update
        public void PreUpdate()
        {
            if (!Main.activeGui.stopsWorldUpdate)
            {
                foreach (EntitySpawner spawner in spawners)
                {
                    if (spawner != null)
                    {
                        if (!spawner.spawned)
                        {
                            if (spawner.type == 0)
                                spawner.SpawnEntity(this);
                            else
                            {
                                float len = (player.position - spawner.bounds.Center.ToVector2()).Length();
                                if (len < spawner.spawnDistance)
                                {
                                    spawner.SpawnEntity(this);
                                }
                            }
                        }
                    }
                }

                foreach (Entity entity in entities)
                {
                    if (entity != null)
                    {
                        entity.PreUpdate(this);
                    }
                }
            }
        }

        public void Update()
        {
            if (!Main.activeGui.stopsWorldUpdate)
            {
                for (int i = hitboxes.Length - 1; i >= 0; i--)
                {
                    Hits.Hit hit = hitboxes[i];

                    if (hit != null)
                    {
                        hit.Update();

                        foreach (IDamageTaker taker in damageTakers.ToList())
                        {
                            if (hit.Collided(taker))
                            {
                                if (taker.TakeDamage(this, hit.damage, hit.type, hit.GetHitDirection(taker)))
                                    damageTakers.Remove(taker);
                            }
                        }

                        if (hit.done)
                        {
                            hitboxes[i] = null;
                        }
                    }
                }

                for (int i = entities.Length - 1; i >= 0; i--)
                {
                    Entity entity = entities[i];
                    if (entity != null)
                    {
                        entity.Update(this);

                        if (entity.dead)
                        {
                            entities[i] = null;
                        }
                    }
                }

                foreach (Trigger trigger in triggers)
                {
                    if (trigger != null)
                    {
                        trigger.Update(this);
                    }
                }
            }

            Update_DEBUG();
        }

        public void Update_DEBUG()
        {
            if (Main.keyboard.KeyPressed(Keys.H))
                Load(name);

            if (Main.DEBUG)
            {
                GuiEditor editor = (GuiEditor)Main.guis["editor"];
                WidgetWindowModeSelector wwms = editor.GetWidgetWindow<WidgetWindowModeSelector>("modeselector");
                int mode = wwms.mode;

                editor.GetWidgetWindow<WidgetWindowEditProperties>("modifyproperties").active = trueSelected != null;
                if (wwms.ModeChanged() || Main.keyboard.KeyPressed(Keys.Escape))
                {
                    editor.GetWidgetWindow<WidgetWindowEditProperties>("modifyproperties").selected = null;

                    trueSelected = null;
                    selected.Clear();

                    editorPoints.Clear();
                }

                if (Main.activeGui == editor)
                {
                    int gridsize = editor.GetGridSize();

                    if (Main.mouse.LeftButtonPressed() && !Main.activeGui.LastClickOnWidget())
                    {
                        Vector2 mouseWorldPos = Main.mouse.GetWorldPosition();
                        mouseWorldPos.X = mouseWorldPos.X.RoundDown(gridsize);
                        mouseWorldPos.Y = mouseWorldPos.Y.RoundDown(gridsize);

                        if (mode != mode_props)
                        {
                            if (editorPoints.Count <= 1)
                            {
                                editorPoints.Add(mouseWorldPos);
                            }
                            else
                            {
                                if (Main.keyboard.KeyHeld(Keys.LeftControl))
                                    editorPoints[0] = mouseWorldPos;
                                else
                                    editorPoints[1] = mouseWorldPos;
                            }
                        }
                        else
                        {
                            if (editorPoints.Count == 0)
                            {
                                editorPoints.Add(mouseWorldPos);
                            }
                            else editorPoints[0] = mouseWorldPos;
                        }
                    }


                    if (mode != mode_props)
                    {
                        #region create/select
                        if (editorPoints.Count == 2)
                        {
                            if (Main.keyboard.KeyPressed(Keys.Enter))
                            {
                                WidgetWindowEditorOptions wweo = editor.GetWidgetWindow<WidgetWindowEditorOptions>("modeoptions");
                                if (mode == mode_brushes)
                                {   //brush mode
                                    WidgetWindowTextureSelector wwts = wweo.GetWindow<WidgetWindowTextureSelector>("brush_textureselector");
                                    Rectangle rect = new Rectangle(editorPoints[0].ToPoint(), (editorPoints[1] - editorPoints[0]).ToPoint());
                                    AddBrush(rect, new TextureInfo(new TextureContainer(wwts.GetTexture()), wwts.GetScale(), wwts.GetColor()), wweo.GetBrushDrawType(), wweo.GetWidget<WidgetCheckbox>("brush_drawahead").isChecked);
                                }
                                else if (mode == mode_walls)
                                {   //wall mode
                                    Rectangle rect = new Rectangle(editorPoints[0].ToPoint(), (editorPoints[1] - editorPoints[0]).ToPoint());
                                    AddWall(rect);
                                }
                                else if (mode == mode_spawners)
                                {   //entity mode
                                    int type = int.Parse(wweo.GetWidget<WidgetTextBox>("entity_type").GetStringSafely("0"));
                                    bool randompos = wweo.GetWidget<WidgetCheckbox>("entity_spawnrandom").isChecked;
                                    float rotation = float.Parse(wweo.GetWidget<WidgetTextBox>("entity_spawnrotation").GetStringSafely("0"));
                                    EnemyNoticeState state = (EnemyNoticeState)wweo.GetWidget<WidgetDropdown>("entity_spawnstate").GetIndex();
                                    Rectangle rect = new Rectangle(editorPoints[0].ToPoint(), (editorPoints[1] - editorPoints[0]).ToPoint());
                                    AddSpawner(new EntitySpawner(rect, type, rotation, randompos, state,
                                        wweo.GetWidget<WidgetTextBox>("entity_info1").GetStringSafely(""), 
                                        wweo.GetWidget<WidgetTextBox>("entity_info2").GetStringSafely("")));
                                }
                                else if (mode == mode_trigger)
                                {
                                    string command = wweo.GetWidget<WidgetTextBox>("trigger_command").GetStringSafely();
                                    string info1 = wweo.GetWidget<WidgetTextBox>("trigger_info1").GetStringSafely();
                                    string info2 = wweo.GetWidget<WidgetTextBox>("trigger_info2").GetStringSafely();
                                    bool perm = wweo.GetWidget<WidgetCheckbox>("trigger_perm").isChecked;
                                    Rectangle rect = new Rectangle(editorPoints[0].ToPoint(), (editorPoints[1] - editorPoints[0]).ToPoint());
                                    AddTrigger(new Trigger(rect, command, info1, info2));
                                }
                                else if (mode == mode_selection)
                                {   //selection mode
                                    Rectangle rect = new Rectangle(editorPoints[0].ToPoint(), (editorPoints[1] - editorPoints[0]).ToPoint());

                                    foreach (Wall wall in walls)
                                    {
                                        if (wall != null)
                                        {
                                            if (rect.Contains(wall.bounds) || rect.Intersects(wall.bounds) || wall.bounds.Contains(rect))
                                            {
                                                selected.Add(wall);
                                            }
                                        }
                                    }

                                    foreach (Brush brush in brushes)
                                    {
                                        if (brush != null)
                                        {
                                            if (rect.Contains(brush.bounds) || rect.Intersects(brush.bounds) || brush.bounds.Contains(rect))
                                            {
                                                selected.Add(brush);
                                            }
                                        }
                                    }

                                    foreach (EntitySpawner spawner in spawners)
                                    {
                                        if (spawner != null)
                                        {
                                            if (rect.Contains(spawner.bounds) || rect.Intersects(spawner.bounds) || spawner.bounds.Contains(rect))
                                            {
                                                selected.Add(spawner);
                                            }
                                        }
                                    }

                                    foreach (Prop prop in props)
                                    {
                                        if (prop != null)
                                        {
                                            if (rect.Contains(prop.position))
                                            {
                                                selected.Add(prop);
                                            }
                                        }
                                    }

                                    foreach (Trigger trigger in triggers)
                                    {
                                        if (trigger != null)
                                        {
                                            if (rect.Contains(trigger.bounds) || rect.Intersects(trigger.bounds) || trigger.bounds.Contains(rect))
                                            {
                                                selected.Add(trigger);
                                            }
                                        }
                                    }
                                }

                                editorPoints.Clear();
                            }
                        }
                        #endregion
                    }
                    else if (mode == 3)
                    {   //prop mode
                        if (editorPoints.Count == 1)
                        {
                            if (Main.keyboard.KeyPressed(Keys.Enter))
                            {
                                WidgetWindowEditorOptions wweo = editor.GetWidgetWindow<WidgetWindowEditorOptions>("modeoptions");
                                WidgetWindowTextureSelector wwts = wweo.GetWindow<WidgetWindowTextureSelector>("prop_textureselector");

                                AddProp(new Prop(editorPoints[0], wwts.GetTextureInfo(), float.Parse(wweo.GetWidget<WidgetTextBox>("prop_shadowscale").GetStringSafely("1"))));
                            }
                        }
                    }

                    if (mode == mode_selection)
                    {
                        #region selectall
                        if (Main.keyboard.KeyModifierPressed(Keys.A, Keys.LeftControl))
                        {
                            foreach (Wall wall in walls)
                            {
                                if (wall != null)
                                {
                                    selected.Add(wall);
                                }
                            }

                            foreach (Brush brush in brushes)
                            {
                                if (brush != null)
                                {
                                    selected.Add(brush);
                                }
                            }

                            foreach (EntitySpawner spawner in spawners)
                            {
                                if (spawner != null)
                                {
                                    selected.Add(spawner);
                                }
                            }

                            foreach (Prop prop in props)
                            {
                                if (prop != null)
                                {
                                    selected.Add(prop);
                                }
                            }

                            foreach (Trigger trigger in triggers)
                            {
                                if (trigger != null)
                                {
                                    selected.Add(trigger);
                                }
                            }
                        }
                        #endregion

                        #region scrollmove
                        int movemode = 0;
                        if (Main.keyboard.KeyModifierPressed(Keys.LeftControl, Keys.OemPlus) || Main.keyboard.KeyModifierHeldAfterTime(Keys.LeftControl, Keys.OemPlus, 60))
                        {
                            movemode = 1;
                        }
                        if (Main.keyboard.KeyModifierPressed(Keys.LeftControl, Keys.OemMinus) || Main.keyboard.KeyModifierHeldAfterTime(Keys.LeftControl, Keys.OemMinus, 60))
                        {
                            movemode = -1;
                        }

                        if (movemode != 0 && trueSelected != null)
                        {
                            if (trueSelected is Brush)
                            {
                                int empty = -1;

                                if (movemode == 1)
                                {
                                    empty = trueSelected.index + 1;

                                    if (empty < brushes.Length)
                                    {
                                        var temp = brushes[empty];
                                        brushes[trueSelected.index] = temp;
                                        if (temp != null)
                                            temp.index = trueSelected.index;
                                        trueSelected.index = empty;
                                        brushes[empty] = (Brush)trueSelected;
                                    }
                                }
                                else if (movemode == -1)
                                {
                                    empty = trueSelected.index - 1;

                                    if (empty > 0)
                                    {
                                        var temp = brushes[empty];
                                        brushes[trueSelected.index] = temp;
                                        if (temp != null)
                                            temp.index = trueSelected.index;
                                        trueSelected.index = empty;
                                        brushes[empty] = (Brush)trueSelected;
                                    }
                                }
                            }
                        }
                        #endregion

                        #region delete selected
                        if ((Main.keyboard.KeyPressed(Keys.Delete) || Main.keyboard.KeyPressed(Keys.Back)) && trueSelected == null)
                        {
                            selected.ForEach(x =>
                            {
                                x.Delete_DEBUG(this);
                            });

                            selected.Clear();
                            trueSelected = null;    //not sure this is really necessary but better safe than sorry
                        }
                        if ((Main.keyboard.KeyPressed(Keys.Delete) || Main.keyboard.KeyPressed(Keys.Back)) && trueSelected != null)
                        {
                            trueSelected.Delete_DEBUG(this);

                            selected.Remove(trueSelected);
                            trueSelected = null;
                        }
                        #endregion

                        #region move selected
                        bool movingselected = false;

                        if (Main.keyboard.KeyModifierPressed(Keys.Up, Keys.LeftControl))
                        {
                            if (trueSelected != null)
                                trueSelected.Move(new Vector2(0, -gridsize));
                            else
                                selected.ForEach(x => x.Move(new Vector2(0, -gridsize)));
                            movingselected = true;
                        }
                        if (Main.keyboard.KeyModifierPressed(Keys.Down, Keys.LeftControl))
                        {
                            if (trueSelected != null)
                                trueSelected.Move(new Vector2(0, gridsize));
                            else
                                selected.ForEach(x => x.Move(new Vector2(0, gridsize)));
                            movingselected = true;
                        }
                        if (Main.keyboard.KeyModifierPressed(Keys.Left, Keys.LeftControl))
                        {
                            if (trueSelected != null)
                                trueSelected.Move(new Vector2(-gridsize, 0));
                            else
                                selected.ForEach(x => x.Move(new Vector2(-gridsize, 0)));
                            movingselected = true;
                        }
                        if (Main.keyboard.KeyModifierPressed(Keys.Right, Keys.LeftControl))
                        {
                            if (trueSelected != null)
                                trueSelected.Move(new Vector2(gridsize, 0));
                            else
                                selected.ForEach(x => x.Move(new Vector2(gridsize, 0)));
                            movingselected = true;
                        }
                        #endregion

                        #region cycle selected
                        if (!movingselected)
                        {
                            if (selected.Count > 0)
                            {
                                if (Main.keyboard.KeyPressed(Keys.Up))
                                {
                                    WidgetWindowEditProperties wwmp = Main.guis["editor"].GetWidgetWindow<WidgetWindowEditProperties>("modifyproperties");

                                    trueSelectedIndex++;
                                    trueSelectedIndex = MathHelper.Clamp(trueSelectedIndex, 0, selected.Count - 1);

                                    wwmp.selected = null;

                                    selected.ForEach(select => select.trueSelected = false);

                                    trueSelected = selected[trueSelectedIndex];
                                    trueSelected.trueSelected = true;
                                    trueSelected.UpdateWindowProperties(wwmp);
                                }
                                else if (Main.keyboard.KeyPressed(Keys.Down))
                                {
                                    WidgetWindowEditProperties wwmp = Main.guis["editor"].GetWidgetWindow<WidgetWindowEditProperties>("modifyproperties");

                                    trueSelectedIndex--;
                                    trueSelectedIndex = MathHelper.Clamp(trueSelectedIndex, 0, selected.Count - 1);

                                    wwmp.selected = null;

                                    selected.ForEach(select => select.trueSelected = false);

                                    trueSelected = selected[trueSelectedIndex];
                                    trueSelected.trueSelected = true;
                                    trueSelected.UpdateWindowProperties(wwmp);
                                }
                            }
                        }
                        #endregion
                    }

                    if (Main.keyboard.KeyModifierPressed(Keys.S, Keys.LeftControl))
                    {
                        Save(editor.GetWidget<WidgetTextBox>("savename").GetStringSafely("unkown") + ".hgmf");
                    }
                    if (Main.keyboard.KeyModifierPressed(Keys.L, Keys.LeftControl))
                    {
                        Load(editor.GetWidget<WidgetTextBox>("savename").GetStringSafely("unkown") + ".hgmf");
                    }
                }
            }
        }

        public void PostUpdate()
        {
            if (!Main.activeGui.stopsWorldUpdate)
            {
                foreach (Entity entity in entities)
                {
                    if (entity != null)
                    {
                        entity.PostUpdate(this);
                    }
                }
            }
        }
        #endregion

        public void MapFunc(int index, Trigger trigger)
        {
            if (name == "citadel1" || name == "citadel1_1")
            {
                if (index == 0)
                {
                    int amt = 0;

                    List<EnemyDoor> doors = new List<EnemyDoor>();
                    foreach (EntitySpawner spawner in spawners)
                    {
                        if (spawner != null)
                        {
                            spawner.attachedEntities.ForEach(x => 
                            {
                                if (x is Undead)
                                {
                                    if (x.dead)
                                        amt++;
                                }
                                else if (x is EnemyDoor)
                                    doors.Add((EnemyDoor)x);
                            });
                        }
                    }

                    if (amt >= 2)
                    {
                        doors.ForEach(x => x.opening = true);   
                    }
                }
                else if (index == 1)
                {
                    if (trigger.triggerTime >= 60)
                    {
                        if (trigger.triggerTime == 60 || trigger.triggerTime == 100 || trigger.triggerTime == 140)
                        {
                            AddHitbox(new HitBox(new Rectangle(trigger.bounds.X, trigger.bounds.Y + trigger.bounds.Height, trigger.bounds.Width, trigger.bounds.Height), 40, 8, 20, StaggerType.KnockdownGetup, null));
                        }
                        if (trigger.triggerTime < 180)
                        {
                            Particle p = AddEntity(new ParticleDust(Main.rand.Next(30, 90),
                                new Vector2(trigger.bounds.X + trigger.bounds.Width, trigger.bounds.Y + trigger.bounds.Height + Main.rand.Next(0, trigger.bounds.Height)),
                                Main.rand.Next(0, 6) == 1 ? Main.rand.Next(12, 24) : Main.rand.Next(3, 8), Main.rand.Next(4, 32),   //1 in 5 chance of spawning a big "rock"
                                Main.rand.Next(0, 360), Color.Brown));
                            if (p != null)
                            {
                                p.maxSpeed = 30;
                                p.velocity.X = -Main.rand.Next(20, 30);
                                p.velocity.Y = Main.rand.NextFloat(-.5f, .5f);
                                p.velocityDecaysTimer = 5;
                            }
                        }
                        else trigger.constant = false;  //stop triggering.
                    }
                }
            }
        }

        #region draw
        public void Draw(SpriteBatch batch)
        {
            if (!Main.activeGui.stopsWorldDraw)
            {
                batch.DrawRectangle(new Rectangle(0, 0, 8192, 8192), backgroundColor);

                foreach (Brush brush in brushes)
                {
                    if (brush != null)
                        if (!brush.drawAhead && brush.drawType != BrushDrawType.WallStretch && brush.drawType != BrushDrawType.WallTile)
                            brush.Draw(batch);
                }

                batch.End();
                batch.Begin(Main.DEBUG ? SpriteSortMode.Deferred : SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, null, Main.camera.GetViewMatrix());

                foreach (Entity entity in entities)
                {
                    if (entity != null)
                        entity.Draw(batch);
                }

                foreach (Prop prop in props)
                {
                    if (prop != null)
                        prop.Draw(batch);
                }

                foreach (Brush brush in brushes)
                {
                    if (brush != null)
                        if (brush.drawAhead || brush.drawType == BrushDrawType.WallStretch || brush.drawType == BrushDrawType.WallTile)
                            brush.Draw(batch);
                }

                batch.End();
                batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, null, Main.camera.GetViewMatrix());

                foreach (Enemy entity in entities.OfType<Enemy>())
                {
                    if (entity != null)
                        entity.DrawHealthbar(batch);
                }
                Draw_DEBUG(batch);
            }
        }

        public void Draw_DEBUG(SpriteBatch batch)
        {
            if (Main.DEBUG)
            {
                if (Main.guis["editor"].GetWidgetWindow<WidgetWindowHolder>("grid").GetWidget<WidgetCheckbox>("showgrid").isChecked)
                {
                    WidgetTextBox gridsizeTB = Main.guis["editor"].GetWidgetWindow<WidgetWindowHolder>("grid").GetWidget<WidgetTextBox>("gridsize");
                    int gridsize = 0;
                    int.TryParse(gridsizeTB.GetStringSafely(), out gridsize);

                    if (gridsize > 0)
                    {
                        for (int x = 0; x <= 8192 / gridsize; x++)
                        {
                            batch.DrawLine(new Vector2(x * gridsize, 0), new Vector2(x * gridsize, 8192), Color.Black);
                        }
                        for (int y = 0; y <= 8192 / gridsize; y++)
                        {
                            batch.DrawLine(new Vector2(0, y * gridsize), new Vector2(8192, y * gridsize), Color.Black);
                        }

                        Vector2 mouseWorldPos = Main.mouse.GetWorldPosition();
                        mouseWorldPos.X = mouseWorldPos.X.RoundDown(gridsize);
                        mouseWorldPos.Y = mouseWorldPos.Y.RoundDown(gridsize);
                        batch.DrawRectangle(new Rectangle(mouseWorldPos.ToPoint(), new Point(gridsize)), Color.Red);
                        batch.DrawString(Main.assets.GetFont("bfMunro12"), mouseWorldPos.ToString(), mouseWorldPos, Color.White);
                    }
                }

                foreach (Brush brush in brushes)
                {
                    if (brush != null)
                        brush.Draw_DEBUG(batch);
                }

                foreach (Wall wall in walls)
                {
                    if (wall != null)
                        wall.Draw_DEBUG(batch);
                }

                foreach (EntitySpawner spawner in spawners)
                {
                    if (spawner != null)
                        spawner.Draw_DEBUG(batch);
                }

                foreach (Prop prop in props)
                {
                    if (prop != null)
                        prop.Draw_DEBUG(batch);
                }

                foreach (Trigger trigger in triggers)
                {
                    if (trigger != null)
                        trigger.Draw_DEBUG(batch);
                }

                foreach (Hits.Hit hit in hitboxes)
                {
                    if (hit != null)
                        hit.Draw_DEBUG(batch);
                }
                foreach (ISelectable selectable in selected)
                    selectable.DrawSelect_DEBUG(batch);

                foreach (Vector2 vec in editorPoints)
                {
                    batch.DrawHollowCircle(vec, 8, Color.Orange, 2, 32);
                }

                if (editorPoints.Count == 2)
                {
                    Rectangle rect = new Rectangle(editorPoints[0].ToPoint(), (editorPoints[1] - editorPoints[0]).ToPoint());
                    batch.DrawHollowRectangle(rect, 2, Color.Orange);
                    batch.DrawString(Main.assets.GetFont("bfMunro12"), "X: " + editorPoints[0].X + " Y: " + editorPoints[0].Y + " W: " + (editorPoints[1].X - editorPoints[0].X) + " H: " + (editorPoints[1].Y - editorPoints[0].Y), editorPoints[0] - new Vector2(0, 16), Color.White);
                }
            }
        }
        #endregion

        #region serialization
        public void Save(string name)
        {
            if (name.EndsWith(".hgmf"))
                this.name = name.Split('.')[0];
            else
            {
                this.name = name;
                name += ".hgmf";
            }
            //this would just overwrite crap before.
            if (File.Exists("maps/" + name))
            {   //map files are so small we can get away with saving 15 backups worth.
                int tries = 15;
                while (tries >= 1)
                {
                    if (File.Exists("maps/" + this.name + "_bkp_" + tries + ".hgmf"))
                    {
                        if (tries != 15)
                        {
                            File.Copy("maps/" + name, "maps/" + this.name + "_bkp_" + (tries + 1) + ".hgmf", true);
                        }
                    }

                    if (tries == 1)
                        File.Copy("maps/" + name, "maps/" + this.name + "_bkp_" + tries + ".hgmf", true);

                    tries--;
                }
            }

            SerWorld world = new SerWorld();

            for (int i = 0; i < brushes.Length; i++)
            {
                if (brushes[i] != null)
                    world.Brushes.Add(brushes[i].Save());
            }

            for (int i = 0; i < walls.Length; i++)
            {
                if (walls[i] != null)
                    world.Walls.Add(walls[i].Save());
            }

            for (int i = 0; i < spawners.Length; i++)
            {
                if (spawners[i] != null)
                    world.EntitySpawners.Add(spawners[i].Save());
            }

            for (int i = 0; i < props.Length; i++)
            {
                if (props[i] != null)
                    world.Props.Add(props[i].Save());
            }

            for (int i = 0; i < triggers.Length; i++)
            {
                if (triggers[i] != null)
                    world.Triggers.Add(triggers[i].Save());
            }

            world.DisplayName = Main.guis["editor"].GetWidget<WidgetTextBox>("displayname").GetStringSafely(displayName);
            world.BackgroundColor = ((GuiEditor)Main.guis["editor"]).GetBackgroundColor().Save();

            if (!Directory.Exists("Maps"))
                Directory.CreateDirectory("Maps");

            using (var output = File.Create("Maps/" + name))
            {
                world.WriteTo(output);
            }
        }

        public void Load(string name)
        {
            if (name.EndsWith(".hgmf"))
                this.name = name.Split('.')[0];
            else
            {
                this.name = name;
                name += ".hgmf";
            }
            collisionWorld = new Humper.World(collisionWorld.Bounds.Width, collisionWorld.Bounds.Height);   //recreate just to be safe; don't want any random leftover IBoxes

            brushes = new Brush[128];
            walls = new Wall[128];
            spawners = new EntitySpawner[128];
            props = new Prop[128];
            triggers = new Trigger[128];
            entities = new Entity[128];
            damageTakers.Clear();

            SerWorld world;

            if (!Directory.Exists("Maps"))
                Directory.CreateDirectory("Maps");

            try
            {
                using (var input = File.OpenRead("Maps/" + name))
                {
                    world = SerWorld.Parser.ParseFrom(input);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("There was an error loading the map file " + name + ".\n" + e);
                return;
            }

            for (int i = 0; i < world.Brushes.Count; i++)
            {
                AddBrush(world.Brushes[i].Load());
            }

            for (int i = 0; i < world.Walls.Count; i++)
            {
                AddWall(world.Walls[i].Load(this));
            }

            for (int i = 0; i < world.EntitySpawners.Count; i++)
            {
                AddSpawner(world.EntitySpawners[i].Load());
            }

            for (int i = 0; i < world.Props.Count; i++)
            {
                AddProp(world.Props[i].Load());
            }

            for (int i = 0; i < world.Triggers.Count; i++)
            {
                AddTrigger(world.Triggers[i].Load());
            }

            displayName = world.DisplayName;
            if (world.BackgroundColor != null)
                backgroundColor = world.BackgroundColor.Load();
        }
        #endregion

        #region add
        public Brush AddBrush(Rectangle bounds, TextureInfo info, BrushDrawType drawType = BrushDrawType.Tile, bool drawAhead = false)
        {
            Brush brush = null;
            for (int i = 0; i < brushes.Length; i++)
            {
                if (brushes[i] == null)
                {
                    brush = new Brush(bounds, info, drawType, drawAhead);
                    brush.index = i;
                    brushes[i] = brush;
                    return brush;
                }
            }

            return brush;
        }

        public Brush AddBrush(Brush b)
        {
            Brush brush = null;
            for (int i = 0; i < brushes.Length; i++)
            {
                if (brushes[i] == null)
                {
                    brush = b;
                    brush.index = i;
                    brushes[i] = brush;
                    return brush;
                }
            }

            return brush;
        }

        public Wall AddWall(Rectangle bounds)
        {
            Wall wall = null;
            for (int i = 0; i < walls.Length; i++)
            {
                if (walls[i] == null)
                {
                    wall = new Wall(this, bounds);
                    wall.index = i;
                    walls[i] = wall;
                    return wall;
                }
            }

            return wall;
        }

        public Wall AddWall(Wall w)
        {
            Wall wall = null;
            for (int i = 0; i < walls.Length; i++)
            {
                if (walls[i] == null)
                {
                    wall = w;
                    wall.index = i;
                    walls[i] = wall;
                    return wall;
                }
            }

            return wall;
        }

        public Prop AddProp(Prop p)
        {
            Prop prop = null;
            for (int i = 0; i < props.Length; i++)
            {
                if (props[i] == null)
                {
                    prop = p;
                    prop.index = i;
                    props[i] = prop;
                    return prop;
                }
            }

            return prop;
        }

        public EntitySpawner AddSpawner(EntitySpawner s)
        {
            EntitySpawner spawner = null;

            for (int i = 0; i < spawners.Length; i++)
            {
                if (spawners[i] == null)
                {
                    spawner = s;
                    spawner.index = i;
                    spawners[i] = spawner;
                    return spawner;
                }
            }

            return spawner;
        }

        public Trigger AddTrigger(Trigger t)
        {
            Trigger trigger = null;

            for (int i = 0; i < triggers.Length; i++)
            {
                if (triggers[i] == null)
                {
                    trigger = t;
                    triggers[i] = trigger;
                    return trigger;
                }
            }

            return trigger;
        }

        public Hits.Hit AddHitbox(Hits.Hit h)
        {
            Hits.Hit hitbox = null;

            for (int i = 0; i < hitboxes.Length; i++)
            {
                if (hitboxes[i] == null)
                {
                    hitbox = h;
                    hitboxes[i] = hitbox;
                    return hitbox;
                }
            }

            return hitbox;
        }

        public T AddEntity<T>(T e) where T : Entity
        {
            T entity = null;
            for (int i = 0; i < brushes.Length; i++)
            {
                if (entities[i] == null || entities[i].dead)
                {
                    entity = e;
                    entities[i] = entity;
                    return entity;
                }
            }

            return entity;
        }

        public T AddEntityLiving<T>(T e) where T : EntityLiving
        {
            T entity = null;
            for (int i = 0; i < brushes.Length; i++)
            {
                if (entities[i] == null || entities[i].dead)
                {
                    entity = e;
                    entities[i] = entity;
                    damageTakers.Add(entity);
                    return entity;
                }
            }

            return entity;
        }
        #endregion
    }
}
