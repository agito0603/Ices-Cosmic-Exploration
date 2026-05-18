using ECommons.GameHelpers;
using ICE.Utilities.Cosmic_Helper;
using System.Collections.Generic;

namespace ICE.Scheduler.Tasks
{
    internal static class GoldCompletionJobSwitcher
    {
        private static readonly HashSet<uint> TriedJobs = new();

        internal static void Reset()
        {
            TriedJobs.Clear();
        }

        internal static bool? TrySwitchToNextJobOrStop()
        {
            const string tag = "[Gold Completion Job Switcher]";

            if (Mission_Settings.Mode != ModeSelect.MissionGoldMode)
                return true;

            var currentJob = Mission_Settings.SelectedJob;
            TriedJobs.Add(currentJob);

            if (!HasAnyNonGoldMissionOnCurrentTerritory())
            {
                IceLogging.ChatInfo("Gold Completion: all missions on the current moon appear to be Gold completed.", "[I.C.E.]");
                SchedulerMain.State = IceState.Idle;
                P.TaskManager.Tasks.Clear();
                Reset();
                return true;
            }

            var nextJob = FindNextJobWithNonGoldMission();

            if (nextJob == 0)
            {
                IceLogging.Info(
                    "Gold Completion: no remaining job with non-Gold missions was found in this scan cycle. Stopping for now.",
                    tag
                );

                SchedulerMain.State = IceState.Idle;
                P.TaskManager.Tasks.Clear();
                Reset();
                return true;
            }

            IceLogging.Info(
                $"Gold Completion: no available non-Gold mission found for current job [{currentJob}]. Switching to job [{nextJob}].",
                tag
            );

            Mission_Settings.SelectedJob = nextJob;
            C.SelectedJob = nextJob;

            P.TaskManager.Tasks.Clear();

            P.TaskManager.Enqueue(() =>
            {
                if ((uint)Player.Job == nextJob)
                    return true;

                if (EzThrottler.Throttle("GoldCompletionJobSwap"))
                    GearsetHandler.TaskClassChange((Job)nextJob);

                return false;
            }, "Changing job for Gold Completion");

            P.TaskManager.EnqueueMulti(
                new(() => Task_CheckMissions.RefreshMissionLibrary(), "Refreshing mission library after Gold Completion job switch"),
                new(() => Task_CheckMissions.OpenMissionUi(), "Opening mission UI after Gold Completion job switch"),
                new(() => Task_CheckMissions.CheckTabs(), "Checking missions after Gold Completion job switch")
            );

            return true;
        }

        private static uint FindNextJobWithNonGoldMission()
        {
            foreach (var jobId in C.JobPrio)
            {
                if (jobId == Mission_Settings.SelectedJob)
                    continue;

                if (TriedJobs.Contains(jobId))
                    continue;

                if (!CosmicHelper.SupportedJobs.Contains(jobId))
                    continue;

                if (Player.GetLevel((Job)jobId) <= 0)
                    continue;

                if (HasNonGoldMissionForJob(jobId))
                    return jobId;
            }

            return 0;
        }

        private static bool HasAnyNonGoldMissionOnCurrentTerritory()
        {
            return CosmicHelper.SheetMissionDict
                .Where(x => x.Value.TerritoryId == Player.Territory.RowId)
                .Any(x => !CosmicHandler.IsMissionGold(x.Key));
        }

        private static bool HasNonGoldMissionForJob(uint jobId)
        {
            return CosmicHelper.SheetMissionDict
                .Where(x => x.Value.TerritoryId == Player.Territory.RowId)
                .Where(x => x.Value.Jobs.Contains(jobId))
                .Where(x => Player.GetLevel((Job)jobId) >= x.Value.Level)
                .Any(x => !CosmicHandler.IsMissionGold(x.Key));
        }
    }
}