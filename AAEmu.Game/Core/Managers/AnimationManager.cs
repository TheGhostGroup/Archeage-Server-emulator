using System;
using System.Collections.Generic;

using AAEmu.Commons.IO;
using AAEmu.Commons.Utils;
using AAEmu.Game.Models.Game.Animation;
using AAEmu.Game.Utils.DB;

using NLog;

namespace AAEmu.Game.Core.Managers
{
    public class AnimationManager : Singleton<AnimationManager>
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();

        private Dictionary<uint, Anim> _animations = new Dictionary<uint, Anim>();
        private Dictionary<string, List<Anim>> _animationsByName = new Dictionary<string, List<Anim>>();

        public Anim GetAnimation(uint id)
        {
            return _animations.ContainsKey(id) ? _animations[id] : null;
        }

        public List<Anim> GetAnimation(string name)
        {
            return _animationsByName.ContainsKey(name) ? _animationsByName[name] : null;
        }

        public void Load()
        {
            _animations = new Dictionary<uint, Anim>();
            _animationsByName = new Dictionary<string, List<Anim>>();

            _log.Info("Loading animations...");

            using (var connection = SQLite.CreateConnection())
            {
                /* Anims */
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM anims";
                    command.Prepare();
                    using (var reader = new SQLiteWrapperReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            var template = new Anim();
                            template.Id = reader.GetUInt32("id");
                            template.Name = reader.GetString("name");
                            template.Loop = reader.GetBoolean("loop");
                            template.Category = (AnimCategory)reader.GetUInt32("category_id");
                            template.RideUB = reader.GetString("ride_ub");
                            template.HangUB = reader.GetString("hang_ub");
                            template.SwimUB = reader.GetString("swim_ub");
                            template.MoveUB = reader.GetString("move_ub");
                            template.RelaxedUB = reader.GetString("relaxed_ub");
                            template.SwimMoveUB = reader.GetString("swim_move_ub");

                            _animations.Add(template.Id, template);
                            //_animationsByName.Add(template.Name, template); // в наличии дубли Name
                            List<Anim> tempListAnim;
                            if (_animationsByName.ContainsKey(template.Name))
                                tempListAnim = _animationsByName[template.Name];
                            else
                            {
                                tempListAnim = new List<Anim>();
                                _animationsByName.Add(template.Name, tempListAnim);
                            }
                            tempListAnim.Add(template);
                        }
                    }
                }
            }

            var contents = FileManager.GetFileContents($"{FileManager.AppPath}Data/anim_durations.json");
            if (string.IsNullOrWhiteSpace(contents))
                _log.Warn(
                    $"File {FileManager.AppPath}Data/anim_durations.json doesn't exist or is empty.");
            else
            {
                if (JsonHelper.TryDeserializeObject(contents, out Dictionary<string, AnimDuration> animDurations, out _))
                    foreach (var key in animDurations.Keys)
                    {
                        foreach (var anim in _animationsByName[key])
                        {
                            //if (!_animationsByName.ContainsKey(key)) continue;
                            //var anim = _animationsByName[key];
                            anim.Duration = animDurations[key].total_time;
                            anim.CombatSyncTime = animDurations[key].combat_sync_time;
                        }
                    }
                else
                    throw new Exception(
                        $"AnimationManager: Parse {FileManager.AppPath}Data/anim_durations.json file");
            }
        }
    }
}
