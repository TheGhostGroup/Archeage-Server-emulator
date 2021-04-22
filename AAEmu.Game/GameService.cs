using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using AAEmu.Commons.Cryptography;
using AAEmu.Game.Core.Managers;
using AAEmu.Game.Core.Managers.Id;
using AAEmu.Game.Core.Managers.UnitManagers;
using AAEmu.Game.Core.Managers.World;
using AAEmu.Game.Core.Network.Game;
using AAEmu.Game.Core.Network.Login;
using AAEmu.Game.Core.Network.Stream;
using AAEmu.Game.Utils.Scripts;

using Microsoft.Extensions.Hosting;

using NLog;

namespace AAEmu.Game
{
    public class GameService : IHostedService, IDisposable
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        public static DateTime StartTime { get; set; }
        public static DateTime EndTime { get; set; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _log.Info("Starting daemon: AAEmu.Game");

            // проверка чтения areasmission0.bai файла
            //NavigationSystem.ReadFromFile(@"g:\Games\Archeage1.2\game_0\main_world\paths\119_034\areasmission0.bai");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            #region Id Managers
            TaskIdManager.Instance.Initialize();
            TaskManager.Instance.Initialize();
            LocalizationManager.Instance.Load();
            ObjectIdManager.Instance.Initialize();
            TradeIdManager.Instance.Initialize();
            #endregion

            #region Gameplay Managers
            ItemIdManager.Instance.Initialize();
            ChatManager.Instance.Initialize();
            CharacterIdManager.Instance.Initialize();
            FamilyIdManager.Instance.Initialize();
            ExpeditionIdManager.Instance.Initialize();
            VisitedSubZoneIdManager.Instance.Initialize();
            PrivateBookIdManager.Instance.Initialize();
            FriendIdManager.Instance.Initialize();
            MateIdManager.Instance.Initialize();
            HousingIdManager.Instance.Initialize();
            HousingTldManager.Instance.Initialize();
            TeamIdManager.Instance.Initialize();
            LaborPowerManager.Instance.Initialize();
            QuestIdManager.Instance.Initialize();

            ZoneManager.Instance.Load();
            WorldManager.Instance.Load();
            var heightmapTask = Task.Run(() =>
            {
                WorldManager.Instance.LoadHeightmaps();
            });
            QuestManager.Instance.Load();

            ShipyardManager.Instance.Load();

            FormulaManager.Instance.Load();
            ExpirienceManager.Instance.Load();
            ConfigurationManager.Instance.Load();

            TlIdManager.Instance.Initialize();
            SpecialtyManager.Instance.Load();
            ItemManager.Instance.Load();
            ItemManager.Instance.LoadUserItems();
            AnimationManager.Instance.Load();
            PlotManager.Instance.Load();
            SkillManager.Instance.Load();
            CraftManager.Instance.Load();
            MateManager.Instance.Load();
            SlaveManager.Instance.Load();
            TeamManager.Instance.Load();
            AuctionManager.Instance.Load();
            MailManager.Instance.Load();

            NameManager.Instance.Load();
            FactionManager.Instance.Load();
            ExpeditionManager.Instance.Load();
            CharacterManager.Instance.Load();
            FamilyManager.Instance.Load();
            PortalManager.Instance.Load();
            FriendMananger.Instance.Load();

            NpcManager.Instance.Load();
            DoodadManager.Instance.Load();
            HousingManager.Instance.Load();
            TransferManager.Instance.Load();
            GimmickManager.Instance.Load();

            await heightmapTask;

            SpawnManager.Instance.Load();
            SpawnManager.Instance.SpawnAll();
            HousingManager.Instance.SpawnAll();
            //TransferManager.Instance.SpawnAll();
            #endregion

            #region Other Managers
            AccessLevelManager.Instance.Load();
            CashShopManager.Instance.Load();
            ScriptCompiler.Compile();

            SaveManager.Instance.Initialize();
            SpecialtyManager.Instance.Initialize();
            BoatPhysicsManager.Instance.Initialize();
            TransferManager.Instance.Initialize();
            GimmickManager.Instance.Initialize();
            SlaveManager.Instance.Initialize();

            //TransferManager.Instance.Initialize();
           
            EncryptionManager.Instance.Load();
            TimeManager.Instance.Start();
            TaskManager.Instance.Start();
            GameNetwork.Instance.Start();
            StreamNetwork.Instance.Start();
            LoginNetwork.Instance.Start();
            #endregion

            StartTime = DateTime.UtcNow;
            stopWatch.Stop();
            _log.Info("Server started! Took {0}", stopWatch.Elapsed);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _log.Info("Stopping daemon ...");

            SaveManager.Instance.Stop();

            SpawnManager.Instance.Stop();
            TaskManager.Instance.Stop();
            GameNetwork.Instance.Stop();
            StreamNetwork.Instance.Stop();
            LoginNetwork.Instance.Stop();

            /*
            HousingManager.Instance.Save();
            MailManager.Instance.Save();
            ItemManager.Instance.Save();
            */
            
            BoatPhysicsManager.Instance.Stop();
            TransferManager.Instance.Stop();
            GimmickManager.Instance.Stop();

            TimeManager.Instance.Stop();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _log.Info("Disposing ...");

            LogManager.Flush();
        }
    }
}
