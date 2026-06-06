using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.GatheringHelper;
using ICE.Utilities.GatheringHelper.RouteLoader;
using ICE.Utilities.ImGuiTools;
using OtterGui;
using OtterGui.Table;
using System.Collections.Generic;
using System.Reflection;
using static ICE.ConfigFiles.Config;
using static ICE.Utilities.Cosmic_Helper.CosmicHelper;

namespace ICE.Ui.MainUi.ModeSelect_Modes.CosmicTable
{
    internal class VerticalCenterColumnString : ColumnString<MissionInfo>
    {
        public override void PreDraw()
        {
            var pos = ImGui.GetCursorPosY();
            var offset = (ImageSize - ImGui.GetTextLineHeight()) / 2;
            ImGui.SetCursorPosY(pos + offset);
        }
    }
    internal class ItemFilterColumn : ColumnFlags<ItemFilter, MissionInfo>
    {
        private ItemFilter[] FlagValues = Array.Empty<ItemFilter>();
        private string[] FlagNames = Array.Empty<string>();

        private string ReturnFlagNames(ItemFilter item)
        {
            return item switch
            {
                ItemFilter.NoItems => "No Items",
                ItemFilter.Enabled => "Enabled",
                ItemFilter.Disabled => "Disabled",
                // ItemFilter.NotCompleted => "Not Completed",
                // ItemFilter.Completed => "Completed",
                // ItemFilter.Gold => "Gold",
                _ => "Unknown",
            };
        }

        protected void SetFlags(params ItemFilter[] flags)
        {
            FlagValues = flags;
            AllFlags = FlagValues.Aggregate((f, g) => f | g);
        }

        protected void SetFlagsAndNames(params ItemFilter[] flags)
        {
            SetFlags(flags);
            SetNames(flags.Select(f => ReturnFlagNames(f)).ToArray());
        }

        protected void SetNames(params string[] names) => FlagNames = names;

        protected sealed override IReadOnlyList<ItemFilter> Values => FlagValues;

        protected sealed override string[] Names => FlagNames;

        public sealed override ItemFilter FilterValue => C.ItemFilter;

        protected sealed override void SetValue(ItemFilter f, bool v)
        {
            var tmp = v ? FilterValue | f : FilterValue & ~f;
            if (tmp == FilterValue)
                return;

            C.ItemFilter = tmp;
            C.SaveDebounced();
        }
    }
    internal class MissionFilterColumn : ColumnFlags<MissionFilter, MissionInfo>
    {
        private MissionFilter[] FlagValues = Array.Empty<MissionFilter>();
        private string[] FlagNames = Array.Empty<string>();

        protected void SetFlags(params MissionFilter[] flags)
        {
            FlagValues = flags;
            AllFlags = FlagValues.Aggregate((f, g) => f | g);
        }

        protected void SetFlagsAndNames(params MissionFilter[] flags)
        {
            SetFlags(flags);
            SetNames(flags.Select(f => f.ToString()).ToArray());
        }

        protected void SetNames(params string[] names) => FlagNames = names;

        protected sealed override IReadOnlyList<MissionFilter> Values => FlagValues;

        protected sealed override string[] Names => FlagNames;

        public sealed override MissionFilter FilterValue => C.MissionFilter;

        protected sealed override void SetValue(MissionFilter f, bool v)
        {
            var tmp = v ? FilterValue | f : FilterValue & ~f;
            if (tmp == FilterValue)
                return;

            C.MissionFilter = tmp;
            C.SaveDebounced();
        }
    }
    internal class JobFilterColumn : ColumnFlags<JobFilter, MissionInfo>
    {
        private JobFilter[] FlagValues = Array.Empty<JobFilter>();
        private string[] FlagNames = Array.Empty<string>();

        protected void SetFlags(params JobFilter[] flags)
        {
            FlagValues = flags;
            AllFlags = FlagValues.Aggregate((f, g) => f | g);
        }

        protected void SetFlagsAndNames(params JobFilter[] flags)
        {
            SetFlags(flags);
            SetNames(flags.Select(f => f.ToString()).ToArray());
        }

        protected void SetNames(params string[] names) => FlagNames = names;

        protected sealed override IReadOnlyList<JobFilter> Values => FlagValues;

        protected sealed override string[] Names => FlagNames;

        public sealed override JobFilter FilterValue => C.JobFilter;

        protected sealed override void SetValue(JobFilter f, bool v)
        {
            var tmp = v ? FilterValue | f : FilterValue & ~f;
            if (tmp == FilterValue)
                return;

            C.JobFilter = tmp;
            C.SaveDebounced();
        }
    }
    internal class Mission_Table : Table<MissionInfo>, IDisposable
    {
        // TODO: Create default width's for all of these...

        public readonly EnabledColumn _enabledColumn;
        public readonly NameColumn _nameColumn = new() { Label = "Name" };
        public readonly IdColumn _idColumn = new() { Label = "ID" };
        public readonly JobColumn _jobColumn = new() { Label = "Job" };
        public readonly MissionColumn _missionColumn = new() { Label = "Rank" };
        public readonly CompletionColumn _completionColumn = new() { Label = "Status" };
        public readonly ClassScoreColumn _classScoreColumn = new() { Label = "Class" };
        public readonly CosmocreditColumn _cosmoColumn = new() { Label = "Cosmo" };
        public readonly LunarCreditColumn _lunarColumn = new() { Label = "Lunar" };
        public readonly DroneCreditColumn _droneColumn = new() { Label = "Dronebits" };
        public readonly PlanetTokensColumn _planetTokenColumn = new() { Label = "Mount" };
        public readonly SPMColumn _spmColumn = new() { Label = "SPM" };
        public readonly TurninColumn _turninColumn = new() { Label = "Goal" };
        public readonly PlanetColumn _planetColumn = new() { Label = "Moons" };
        public readonly ProfileColumn _profileColumn = new() { Label = "Profile" };
        public readonly NotesColumn _notesColumn = new() { Label = "Notes" };
        public readonly AllRelicExpColum _allExpColumn = new() { Label = "Exp" };

        public Mission_Table(List<MissionInfo> itemList) : base("Item_Table_V2", itemList)
        {
            _enabledColumn = new EnabledColumn(this) { Label = "Enabled" };

            List<Column<MissionInfo>> headers = [
                _enabledColumn, _completionColumn, _idColumn, _planetColumn,
                _jobColumn, _missionColumn, _nameColumn, _classScoreColumn, 
                _cosmoColumn, _lunarColumn, _droneColumn, _planetTokenColumn, 
                _spmColumn,  _turninColumn, _allExpColumn];


            var tierFlags = new (int tier, ItemFilter flag)[]
            {
                (1, ItemFilter.HasI),   (2, ItemFilter.HasII),  (3, ItemFilter.HasIII),
                (4, ItemFilter.HasIV),  (5, ItemFilter.HasV),   (6, ItemFilter.HasVI),
                (7, ItemFilter.HasVII)
            };

            foreach (var (tier, flag) in tierFlags)
            {
                string tierName = tier switch { 1 => "I", 2 => "II", 3 => "III", 4 => "IV", 5 => "V", 6 => "VI", 7 => "VII", _ => "?" };
                headers.Add(new RelicExpColumn(tier, flag) { Label = $"Exp {tierName}" });
            }

            headers.Add(_profileColumn, _notesColumn);
            this.Headers = [.. headers];

            Sortable = true;
            Flags |= ImGuiTableFlags.Hideable | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Resizable | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit;
        }

        public void Dispose()
        {

        }

        public sealed class EnabledColumn : ItemFilterColumn
        {
            public override float Width => Math.Max(
                ImGui.CalcTextSize(Label + "xxx").X + ImGui.GetStyle().CellPadding.X * 2,
                ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2
            );
            private readonly Mission_Table _table;
            public EnabledColumn(Mission_Table table)
            {
                _table = table;
                Flags = ImGuiTableColumnFlags.NoHide | ImGuiTableColumnFlags.NoResize;
                SetFlags(ItemFilter.Enabled, ItemFilter.Disabled);
                SetNames("Enabled", "Disabled");
            }

            public override int Compare(MissionInfo lhs, MissionInfo rhs)
                => lhs.Enabled().CompareTo(rhs.Enabled());

            public override void DrawColumn(MissionInfo item, int _)
            {
                ImGui.PushID(item.Id);

                bool disabled = C.SelectedMode == ModeSelect.MissionGoldMode
                             || C.SelectedMode == ModeSelect.LevelMode
                             || (C.SelectedMode == ModeSelect.RelicMode && !C.XPRelicOnlyEnabled);

                if (!disabled)
                {
                    bool enabled = C.MissionConfig[item.Id].Enabled;
                    if (ImGui_Ice.Table_CenterCheckbox("##EnableMission", ref enabled))
                    {
                        C.MissionConfig[item.Id].Enabled = enabled;
                        if (enabled == true)
                        {
                            foreach (var prevMission in CosmicHelper.SheetMissionDict[item.Id].SequenceMissions_Previous)
                            {
                                C.MissionConfig[prevMission].Enabled = true;
                            }
                        }

                        C.SaveDebounced();
                        _table.SetFilterDirty();
                    }
                    if (ImGui.IsItemClicked())
                    {
                        Window_ExternalDetails.SelectedMission = item.Id;
                    }
                }
                ImGui.PopID();
            }

            public override bool FilterFunc(MissionInfo item)
            {
                return item.Enabled() ? FilterValue.HasFlag(ItemFilter.Enabled) : FilterValue.HasFlag(ItemFilter.Disabled);
            }
        }
        public sealed class NameColumn : VerticalCenterColumnString
        {
            public NameColumn() => Flags |= ImGuiTableColumnFlags.NoHide;
            public override string ToName(MissionInfo mission) => mission.SheetInfo.Name;
            public override void DrawColumn(MissionInfo mission, int _)
            {
                if (UnsupportedMissions.Ids.Contains(mission.Id))
                {
                    ImGuiEx.IconWithTooltip(FontAwesomeIcon.ExclamationTriangle, "This mission is not currently supported\n" +
                        "Had to rework the gathering dictionary and I'm tired of people not reading, so I had to push the update\n" +
                        "Sooner rather than later. I should have it done Sunday though if not tonight\n" +
                        "Sorry for the conconvience");
                }
                if (mission.SheetInfo.TerritoryId == CosmicMoonRegistry.Auxesia.TerritoryId && mission.SheetInfo.Jobs.Contains(18))
                {
                    ImGuiEx.IconWithTooltip(FontAwesomeIcon.ExclamationTriangle,
                        "Fishing isn't *-directly-* supportet yet. But all the fishing holes have locations now\n" +
                        "So you should be able to import from the autohook wiki -> Set the first preset's name under \"Fishing Settings\"\n" +
                        "And be able to farm to your hearts content. About as close as I can do rn till I finish up the rest of btn/min");
                }

                if (ImGui.Button(mission.SheetInfo.Name))
                {
                    IceLogging.Verbose("Testing... if this fires off multiple times", "DEBUG TEST");
                    Window_ExternalDetails.SelectedMission = mission.Id;
                    P.externalDetails.IsOpen = true;
                    IceLogging.Verbose($"Collasped condition: {P.externalDetails.CollapsedCondition.ToString()}");
                }
                if (mission.SheetInfo.Attributes.HasFlag(MissionAttributes.Gather) || mission.SheetInfo.Attributes.HasFlag(MissionAttributes.Fish))
                {
                    ImGui.SameLine();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Flag, $"Flag_{mission.Id}"))
                    {
                        Window_ExternalDetails.SelectedMission = mission.Id;
                        Utils.SetGatheringRing(mission.SheetInfo.TerritoryId, (int)mission.SheetInfo.MapPosition.X, (int)mission.SheetInfo.MapPosition.Y, mission.SheetInfo.Radius, mission.SheetInfo.Name);
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text($"X: {mission.SheetInfo.MapPosition.X:N0}, Z: {mission.SheetInfo.MapPosition.Y:N0}");
                        ImGui.EndTooltip();
                    }
                }
                if (CosmicHelper.CriticalLocations.TryGetValue(mission.Id, out var criticalLoc))
                {
                    ImGui.SameLine();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.FlagCheckered, $"CriticalFlag_{mission.Id}"))
                    {
                        Utils.SetFlagForNPC(mission.SheetInfo.TerritoryId, criticalLoc.MapInfo.X, criticalLoc.MapInfo.Y);
                    }
                }
            }
        }
        public sealed class IdColumn : VerticalCenterColumnString
        {
            public override float Width => Math.Max(
                ImGui.CalcTextSize(Label + "xxx").X + ImGui.GetStyle().CellPadding.X * 2,
                ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2
            );
            public IdColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
            }
            public override string ToName(MissionInfo item) => item.Id.ToString();
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.Id.CompareTo(rhs.Id);

            public override void DrawColumn(MissionInfo item, int _)
            {
                var mission = CosmicHelper.CurrentLunarMission;

                if (mission != 0 && mission == item.Id)
                {
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, ImGui.GetColorU32(new Vector4(0.0f, 1.0f, 0.2f, 0.25f)));
                }
                else if (CosmicHandler.All_AvailableMissions().Contains(item.Id))
                {
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, ImGui.GetColorU32(new Vector4(0.0f, 1.0f, 0.2f, 0.25f)));
                }

                ImGuiUtil.Center($"{item.Id}");
            }
        }
        public sealed class CompletionColumn : ItemFilterColumn
        {
            public CompletionColumn()
            {
                SetFlags(ItemFilter.NotCompleted, ItemFilter.Completed, ItemFilter.Gold);
                SetNames("Not Completed", "Completed", "Gold");
            }
            public override float Width => Math.Max(
                ImGui.CalcTextSize(Label + "xxx").X + ImGui.GetStyle().CellPadding.X * 2,
                ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2
            );
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.CompletionStatus.CompareTo(rhs.SheetInfo.CompletionStatus);
            public override void DrawColumn(MissionInfo item, int idx)
            {
                var status = item.SheetInfo.CompletionStatus;
                var frameHeight = ImGui.GetFrameHeight();
                var size = new Vector2(frameHeight);

                var columnWidth = ImGui.GetColumnWidth();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (columnWidth - frameHeight) / 2);

                if (status is Status.Gold)
                {
                    if (Svc.Texture.GetFromGame("ui/uld/WKSMission_hr1.tex") is { } tex && tex.TryGetWrap(out var wrap, out _))
                    {
                        ImGui.Image(wrap.Handle, size, new Vector2(0.2347f, 0.3500f), new Vector2(0.2959f, 0.6500f));
                    }
                }
                else
                {
                    var icon = status is Status.None ? FontAwesomeIcon.Times : FontAwesomeIcon.Check;
                    var color = status is Status.None ? EColor.Red : EColor.Green;

                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetStyle().FramePadding.X);

                    using (ImRaii.PushFont(UiBuilder.IconFont))
                    using (ImRaii.PushColor(ImGuiCol.Text, color))
                    {
                        ImGuiEx.Icon(icon);
                    }
                }
            }
            public override bool FilterFunc(MissionInfo item)
            {
                var status = item.SheetInfo.CompletionStatus;

                if (FilterValue.HasFlag(ItemFilter.NotCompleted) && status == Status.None) return true;
                if (FilterValue.HasFlag(ItemFilter.Completed) && status == Status.Completed) return true;
                if (FilterValue.HasFlag(ItemFilter.Gold) && status == Status.Gold) return true;

                return false;
            }
        }
        public sealed class ClassScoreColumn : VerticalCenterColumnString
        {
            public ClassScoreColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
            }
            public override float Width => Math.Max(
                ImGui.CalcTextSize(Label + "xxx").X + ImGui.GetStyle().CellPadding.X * 2,
                ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2
            );
            public override string ToName(MissionInfo mission) => mission.SheetInfo.ClassScore.ToString();
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.ClassScore.CompareTo(rhs.SheetInfo.ClassScore);
            public override void DrawColumn(MissionInfo mission, int _)
            {
                ImGuiUtil.Center($"{mission.SheetInfo.ClassScore}");
            }
        }
        public sealed class CosmocreditColumn : VerticalCenterColumnString
        {
            public CosmocreditColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
            }
            public override float Width => Math.Max(
                ImGui.CalcTextSize(Label + "xxx").X + ImGui.GetStyle().CellPadding.X * 2,
                ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2
            );
            public override string ToName(MissionInfo mission) => mission.SheetInfo.CosmoCredit.ToString();
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.CosmoCredit.CompareTo(rhs.SheetInfo.CosmoCredit);
            public override void DrawColumn(MissionInfo mission, int _)
            {
                ImGuiUtil.Center($"{mission.SheetInfo.CosmoCredit}");
            }
        }
        public sealed class LunarCreditColumn : VerticalCenterColumnString
        {
            public LunarCreditColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
            }
            public override float Width => Math.Max(
                ImGui.CalcTextSize(Label + "xxx").X + ImGui.GetStyle().CellPadding.X * 2,
                ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2
            );
            public override string ToName(MissionInfo mission) => mission.SheetInfo.LunarCredit.ToString();
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.LunarCredit.CompareTo(rhs.SheetInfo.LunarCredit);
            public override void DrawColumn(MissionInfo mission, int _)
            {
                ImGuiUtil.Center($"{mission.SheetInfo.LunarCredit}");
            }
        }
        public sealed class DroneCreditColumn : VerticalCenterColumnString
        {
            public DroneCreditColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
            }
            public override float Width => Math.Max(
                ImGui.CalcTextSize(Label + "xxx").X + ImGui.GetStyle().CellPadding.X * 2,
                ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2
            );
            public override string ToName(MissionInfo mission) => mission.SheetInfo.DronebitReward.ToString();
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.DronebitReward.CompareTo(rhs.SheetInfo.DronebitReward);
            public override void DrawColumn(MissionInfo mission, int _)
            {
                ImGuiUtil.Center($"{mission.SheetInfo.DronebitReward}");
            }
        }
        public sealed class PlanetTokensColumn : ItemFilterColumn
        {
            public PlanetTokensColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
                SetFlags(ItemFilter.HasTokens, ItemFilter.NoTokens);
                SetNames("Has Tokens", "No Tokens");
            }
            public override float Width => Math.Max(
                ImGui.CalcTextSize(Label + "xxx").X + ImGui.GetStyle().CellPadding.X * 2,
                ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2
            );
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.TokenItemAmount.CompareTo(rhs.SheetInfo.TokenItemAmount);
            public override void DrawColumn(MissionInfo mission, int _)
            {
                ImGuiUtil.Center($"{mission.SheetInfo.TokenItemAmount}");
            }

            public override bool FilterFunc(MissionInfo mission)
            {
                return mission.SheetInfo.TokenItemAmount > 0 ? FilterValue.HasFlag(ItemFilter.HasTokens) : FilterValue.HasFlag(ItemFilter.NoTokens);
            }
        }
        public sealed class AllRelicExpColum : ItemFilterColumn
        {
            public AllRelicExpColum()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
                SetFlags(ItemFilter.HasI, ItemFilter.HasII, ItemFilter.HasIII, ItemFilter.HasIV, ItemFilter.HasV, ItemFilter.HasVI, ItemFilter.HasVII);
                SetNames("I", "II", "III", "IV", "V", "VI", "VII");
            }

            public override int Compare(MissionInfo lhs, MissionInfo rhs)
            {
                var lhsPairs = lhs.SheetInfo.RelicXpInfo.OrderBy(x => x.Key).ThenBy(x => x.Value).FirstOrDefault();
                var rhsPairs = rhs.SheetInfo.RelicXpInfo.OrderBy(x => x.Key).ThenBy(x => x.Value).FirstOrDefault();

                var keyCompare = lhsPairs.Key.CompareTo(rhsPairs.Key);
                if (keyCompare != 0) return keyCompare;

                return lhsPairs.Value.CompareTo(rhsPairs.Value);
            }

            private static string RomanNumeral(int tier) => tier switch
            {
                1 => "I",
                2 => "II",
                3 => "III",
                4 => "IV",
                5 => "V",
                6 => "VI",
                7 => "VII",
                _ => "?"
            };

            public override void DrawColumn(MissionInfo item, int idx)
            {
                var exps = item.SheetInfo.RelicXpInfo
                    .Where(x => x.Value != 0)
                    .OrderBy(x => x.Key)
                    .ToList();

                if (exps.Count == 0)
                {
                    ImGuiUtil.Center("-");
                    return;
                }

                float spacing = ImGui.GetStyle().ItemSpacing.X;
                float padding = ImGui.GetStyle().FramePadding.X;

                // Calculate total width of all pills + spacing between them
                float totalWidth = exps.Sum(x => ImGui.CalcTextSize($"{RomanNumeral(x.Key)}:{x.Value}").X + padding * 2);
                totalWidth += spacing * (exps.Count - 1);

                float columnWidth = ImGui.GetColumnWidth();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (columnWidth - totalWidth) / 2);

                for (int i = 0; i < exps.Count; i++)
                {
                    var (tier, value) = (exps[i].Key, exps[i].Value);

                    Vector4 pillColor = tier switch
                    {
                        1 => new Vector4(0.9f, 0.8f, 0.1f, 0.8f), // I   - Yellow
                        2 => new Vector4(0.9f, 0.5f, 0.1f, 0.8f), // II  - Orange
                        3 => new Vector4(0.8f, 0.2f, 0.2f, 0.8f), // III - Red
                        4 => new Vector4(0.6f, 0.2f, 0.8f, 0.8f), // IV  - Purple
                        5 => new Vector4(0.2f, 0.4f, 0.9f, 0.8f), // V   - Blue
                        6 => new Vector4(0.4f, 0.8f, 1.0f, 0.8f), // VI  - Light Blue
                        7 => new Vector4(0.2f, 0.8f, 0.3f, 0.8f), // VII - Green
                        _ => new Vector4(0.5f, 0.5f, 0.5f, 0.8f),
                    };

                    string roman = tier switch
                    {
                        1 => "I",
                        2 => "II",
                        3 => "III",
                        4 => "IV",
                        5 => "V",
                        6 => "VI",
                        7 => "VII",
                        _ => "?"
                    };


                    using (ImRaii.PushColor(ImGuiCol.Button, pillColor)
                                 .Push(ImGuiCol.ButtonHovered, pillColor with { W = 1.0f })
                                 .Push(ImGuiCol.ButtonActive, pillColor))
                    {
                        ImGui.SmallButton($"{roman}:{value}##exp{tier}_{idx}");
                    }

                    if (i < exps.Count - 1)
                        ImGui.SameLine();
                }
            }

            public override bool FilterFunc(MissionInfo mission)
            {
                var exps = mission.SheetInfo.RelicXpInfo.Where(x => x.Value > 0).ToList();

                if (exps.Count == 0) return true;

                return exps.Any(x => FilterValue.HasFlag(TierToFlag(x.Key)));
            }
        }
        public sealed class RelicExpColumn : ItemFilterColumn
        {
            private readonly int _tier;
            private readonly ItemFilter _flag;

            public RelicExpColumn(int tier, ItemFilter flag)
            {
                Flags = ImGuiTableColumnFlags.NoResize;
                _tier = tier;
                _flag = flag;
                SetFlags(ItemFilter.HasI, ItemFilter.HasII, ItemFilter.HasIII, ItemFilter.HasIV, ItemFilter.HasV, ItemFilter.HasVI, ItemFilter.HasVII);
                SetNames("I", "II", "III", "IV", "V", "VI", "VII");
            }
            public override float Width => Math.Max(
                ImGui.CalcTextSize(Label + "xxx").X + ImGui.GetStyle().CellPadding.X * 2,
                ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2
            );

            public override int Compare(MissionInfo lhs, MissionInfo rhs)
                => lhs.SheetInfo.RelicXpInfo.GetValueOrDefault(_tier)
                   .CompareTo(rhs.SheetInfo.RelicXpInfo.GetValueOrDefault(_tier));

            public override void DrawColumn(MissionInfo mission, int _)
            {
                var value = mission.SheetInfo.RelicXpInfo.GetValueOrDefault(_tier);

                if (value == 0)
                {
                    ImGuiUtil.Center("-");
                    return;
                }

                Vector4 pillColor = _tier switch
                {
                    1 => new Vector4(0.3f, 0.5f, 0.8f, 0.8f),
                    2 => new Vector4(0.3f, 0.7f, 0.5f, 0.8f),
                    3 => new Vector4(0.7f, 0.6f, 0.2f, 0.8f),
                    4 => new Vector4(0.7f, 0.3f, 0.6f, 0.8f),
                    5 => new Vector4(0.8f, 0.4f, 0.2f, 0.8f),
                    6 => new Vector4(0.8f, 0.2f, 0.2f, 0.8f),
                    7 => new Vector4(0.6f, 0.8f, 0.9f, 0.8f),
                    _ => new Vector4(0.5f, 0.5f, 0.5f, 0.8f),
                };

                var buttonWidth = ImGui.CalcTextSize($"{value}").X + ImGui.GetStyle().FramePadding.X * 2;
                var columnWidth = ImGui.GetColumnWidth();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (columnWidth - buttonWidth) / 2);

                using (ImRaii.PushColor(ImGuiCol.Button, pillColor)
                             .Push(ImGuiCol.ButtonHovered, pillColor with { W = 1.0f })
                             .Push(ImGuiCol.ButtonActive, pillColor))
                {
                    ImGui.SmallButton($"{value}##exp{_tier}");
                }
            }

            public override bool FilterFunc(MissionInfo mission)
            {
                var exps = mission.SheetInfo.RelicXpInfo.Where(x => x.Value > 0).ToList();

                if (exps.Count == 0) return true;

                return exps.Any(x => FilterValue.HasFlag(TierToFlag(x.Key)));
            }
        }
        public sealed class MissionColumn : MissionFilterColumn
        {
            public MissionColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
                SetFlags(MissionFilter.RedAlert, MissionFilter.Sequence, MissionFilter.Weather, MissionFilter.Timed, MissionFilter.ARank, MissionFilter.BRank, MissionFilter.CRank, MissionFilter.DRank, MissionFilter.Master);
                SetNames("Red Alert", "Sequence", "Weather", "Timed", "A Rank", "B Rank", "C Rank", "D Rank", "Master");
            }
            public override float Width => Math.Max(ImGui.CalcTextSize(Label + "XX").X + ImGui.GetStyle().CellPadding.X * 2, ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2);

            private static int GetMissionPriority(CosmicInfo info)
            {
                if (info.IsCritical) return 10;
                if (info.IsSequence) return 9;
                if (info.IsWeather) return 8;
                if (info.IsTimed) return 7;
                // Rank 6 = Provisional (handled above), 5 = Ex, 4 = A, 3 = B, 2 = C, 1 = D
                return (int)info.Rank;
            }

            public override int Compare(MissionInfo lhs, MissionInfo rhs) =>
                GetMissionPriority(lhs.SheetInfo).CompareTo(GetMissionPriority(rhs.SheetInfo));
            public override void DrawColumn(MissionInfo item, int idx)
            {
                var status = item.SheetInfo.CompletionStatus;
                var frameHeight = ImGui.GetFrameHeight();
                var size = new Vector2(frameHeight);

                var columnWidth = ImGui.GetColumnWidth();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (columnWidth - frameHeight) / 2);

                if (item.SheetInfo.IsCritical || item.SheetInfo.IsWeather)
                {
                    var texture = item.SheetInfo.IsCritical ?
                        Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), "ICE.Resources.Red_Alert.png").GetWrapOrEmpty()
                      : CosmicHelper.WeatherIconDict[item.SheetInfo.Weather].GetWrapOrEmpty();

                    ImGui.Image(texture.Handle, size);
                }
                else
                {
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetStyle().FramePadding.X);
                    if (item.SheetInfo.IsProvisional)
                    {
                        var icon = item.SheetInfo.IsTimed ? FontAwesomeIcon.Clock : FontAwesomeIcon.ListOl;
                        ImGuiEx.Icon(icon);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            ImGui.Text("Time Slot");
                            ImGui.Text($"{item.SheetInfo.StartTime:D2}:00 - {item.SheetInfo.EndTime:D2}:00");
                            ImGui.EndTooltip();
                        }
                    }
                    else
                    {
                        string rank = item.SheetInfo.Rank switch
                        {
                            6 => "M",
                            5 or 4 => "A",
                            3 => "B",
                            2 => "C",
                            1 => "D",
                            _ => "???"
                        };
                        ImGui.Text(rank);
                    }
                }
            }
            public override bool FilterFunc(MissionInfo item)
            {
                var sheetInfo = item.SheetInfo;
                bool special = sheetInfo.IsProvisional || sheetInfo.IsCritical;

                if (FilterValue.HasFlag(MissionFilter.RedAlert) && sheetInfo.IsCritical) return true;
                if (FilterValue.HasFlag(MissionFilter.Sequence) && sheetInfo.IsSequence) return true;
                if (FilterValue.HasFlag(MissionFilter.Timed) && sheetInfo.IsTimed) return true;
                if (FilterValue.HasFlag(MissionFilter.Weather) && sheetInfo.IsWeather) return true;
                if (FilterValue.HasFlag(MissionFilter.ARank) && sheetInfo.ARank && !special) return true;
                if (FilterValue.HasFlag(MissionFilter.BRank) && sheetInfo.BRank && !special) return true;
                if (FilterValue.HasFlag(MissionFilter.CRank) && sheetInfo.CRank && !special) return true;
                if (FilterValue.HasFlag(MissionFilter.DRank) && sheetInfo.Drank && !special) return true;
                if (FilterValue.HasFlag(MissionFilter.Master) && sheetInfo.Master) return true;

                return false;
            }
        }
        public sealed class SPMColumn : VerticalCenterColumnString
        {
            public SPMColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
            }
            public override float Width => Math.Max(
                ImGui.CalcTextSize(Label + "xxx").X + ImGui.GetStyle().CellPadding.X * 2,
                ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2
            );
            private double GetScore(MissionInfo item)
            {
                var scoreInfo = item.SheetInfo.ScoreInfo();
                if (item.SheetInfo.IsCritical)
                    return scoreInfo[TurninState.Critical].Score;
                if (scoreInfo.TryGetValue(TurninState.SequenceGold, out var seqGold) && seqGold.Score != 0)
                    return seqGold.Score;
                return scoreInfo.Values.MaxBy(r => r.Score)?.Score ?? 0;
            }

            public override string ToName(MissionInfo item) => $"{GetScore(item):N2}";
            public override int Compare(MissionInfo x, MissionInfo y) => GetScore(x).CompareTo(GetScore(y));
            public override void DrawColumn(MissionInfo item, int _)
            {
                var scoreInfo = item.SheetInfo.ScoreInfo();
                var bestScore = scoreInfo.MaxBy(r => r.Value.Score);
                if (scoreInfo.TryGetValue(TurninState.SequenceGold, out var seqGold) && seqGold.Score != 0)
                    bestScore = new(TurninState.SequenceGold, seqGold);

                if (bestScore.Value.Score != 0)
                {
                    string scoreText = $"{bestScore.Value.Score:N2}";

                    var buttonWidth = ImGui.CalcTextSize(scoreText).X + ImGui.GetStyle().FramePadding.X * 2;
                    var columnWidth = ImGui.GetColumnWidth();
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (columnWidth - buttonWidth) / 2);

                    Vector4 pillColor = bestScore.Key switch
                    {
                        TurninState.Bronze => new(0.6f, 0.35f, 0.15f, 1.0f), // darker bronze
                        TurninState.Silver => new(0.6f, 0.6f, 0.6f, 1.0f),
                        TurninState.Gold => new(0.85f, 0.70f, 0.0f, 1.0f), // slightly muted gold
                        TurninState.Critical => new(0.7f, 0.1f, 0.9f, 1.0f), // purple feels "special"
                        TurninState.SequenceGold => new(0.95f, 0.60f, 0.0f, 1.0f),  // amber-gold, more orange warmth
                        _ => new(0.5f, 0.5f, 0.5f, 0.8f)
                    };

                    bool isBright = bestScore.Key is TurninState.Gold or TurninState.Silver or TurninState.SequenceGold;
                    Vector4 textColor = isBright ? new(0.1f, 0.1f, 0.1f, 1.0f) : new(1.0f, 1.0f, 1.0f, 1.0f);

                    using (ImRaii.PushColor(ImGuiCol.Button, pillColor)
                        .Push(ImGuiCol.ButtonHovered, pillColor with { W = 1.0f })
                        .Push(ImGuiCol.ButtonActive, pillColor)
                        .Push(ImGuiCol.Text, textColor))
                    {
                        ImGui.SmallButton(scoreText);
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text($"[Average] Rewards per minute");
                        if (ImGui.BeginTable($"Score Info Table_{item.SheetInfo.MissionId}", 5, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
                        {
                            ImGui.TableSetupColumn("Kind");
                            ImGui.TableSetupColumn("Score");
                            ImGui.TableSetupColumn("Credits");
                            ImGui.TableSetupColumn("Planetary");
                            ImGui.TableSetupColumn("Tokens");

                            ImGui.TableHeadersRow();

                            foreach (var entry in scoreInfo.Where(x => x.Value.Score != 0))
                            {
                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(0);
                                ImGui.Text($"{entry.Key} [{entry.Value.Completions:N0}]");

                                ImGui.TableNextColumn();
                                ImGui.Text($"{entry.Value.Score:N2}");

                                ImGui.TableNextColumn();
                                ImGui.Text($"{entry.Value.Cosmocredit:N2}");

                                ImGui.TableNextColumn();
                                ImGui.Text($"{entry.Value.PlanetCredits:N2}");

                                ImGui.TableNextColumn();
                                string tokens = entry.Value.Tokens > 0 ? $"{entry.Value.Tokens:N2}" : "-";
                                ImGui.Text(tokens);
                            }

                            ImGui.EndTable();
                        }
                        ImGui.EndTooltip();
                    }
                }
                else
                {
                    ImGuiUtil.Center("-");
                }

            }
        }
        public sealed class TurninColumn : ItemFilterColumn
        {
            public TurninColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
                SetFlags(ItemFilter.TurninGold, ItemFilter.TurninSilver, ItemFilter.TurninBronze);
                SetNames("Gold", "Silver", "Bronze");
            }
            public override float Width
            {
                get
                {
                    int amount = C.MissionFilter.HasFlag(MissionFilter.Master) ? 4 : 3;

                    var iconWidth = ImGui.GetFrameHeight(); // IconButton is square, frameHeight x frameHeight
                    var spacing = ImGui.GetStyle().ItemSpacing.X;
                    var cellPadding = ImGui.GetStyle().CellPadding.X * 2;

                    var headerWidth = ImGui.CalcTextSize(Label).X + cellPadding;
                    var contentWidth = iconWidth * amount + spacing * 3 + cellPadding; // 4 icons (clock+3 trophies) worst case

                    return Math.Max(headerWidth, contentWidth);
                }
            }
            public override int Compare(MissionInfo lhs, MissionInfo rhs)
            {
                if (C.MissionConfig.TryGetValue(lhs.Id, out var lhsConfig) && C.MissionConfig.TryGetValue(rhs.Id, out var rhsConfig))
                {
                    return lhsConfig.TurninGoal.CompareTo(rhsConfig.TurninGoal);
                }
                else
                {
                    return 0;
                }
            }
            public override void DrawColumn(MissionInfo item, int idx)
            {
                if (item.SheetInfo.Attributes.HasFlag(MissionAttributes.Score_TimeRemaining) || item.SheetInfo.IsCritical)
                    ImGuiUtil.Center("Auto");
                else
                {
                    Vector4 BronzeColor = new Vector4(0.804f, 0.498f, 0.196f, 1.0f);
                    Vector4 SilverColor = new Vector4(0.753f, 0.753f, 0.753f, 1.0f);
                    Vector4 GoldColor = new Vector4(1.0f, 0.843f, 0.0f, 1.0f);
                    Vector4 DisabledColor = new Vector4(0.4f, 0.4f, 0.4f, 1.0f);

                    ImGui.PushID($"Mission_{item.Id}");

                    if (C.MissionConfig.TryGetValue(item.Id, out var configInfo))
                    {
                        var highestTurnin = configInfo.TurninGoal;
                        var timeExpired = highestTurnin == TurninState.TimeExpired;
                        var goldEnabled = !timeExpired && highestTurnin >= TurninState.Gold;
                        var silverEnabled = !timeExpired && highestTurnin >= TurninState.Silver;
                        var bronzeEnabled = !timeExpired && highestTurnin >= TurninState.Bronze;

                        if (item.SheetInfo.Rank == 6 && !item.SheetInfo.IsProvisional)
                        {
                            using (ImRaii.PushColor(ImGuiCol.Text, timeExpired ? GoldColor : DisabledColor))
                            {
                                if (ImGuiEx.IconButton(FontAwesomeIcon.Clock, "##TimeExpired"))
                                {
                                    configInfo.TurninGoal = TurninState.TimeExpired;
                                    C.SaveDebounced();
                                }
                            }
                            if (ImGui.IsItemHovered())
                                ImGui.SetTooltip("Only turn in when the mission timer expires (keep gathering for max score).\nUseful for Tool Mastery missions that extend their timer on goal completion.");
                            ImGui.SameLine();
                        }
                        using (ImRaii.PushColor(ImGuiCol.Text, goldEnabled ? GoldColor : DisabledColor))
                        {
                            if (ImGuiEx.IconButton(FontAwesomeIcon.Trophy, "##Gold"))
                            {
                                configInfo.TurninGoal = TurninState.Gold;
                                C.SaveDebounced();
                            }
                        }
                        ImGui.SameLine();
                        using (ImRaii.PushColor(ImGuiCol.Text, silverEnabled ? SilverColor : DisabledColor))
                        {
                            if (ImGuiEx.IconButton(FontAwesomeIcon.Trophy, "##Silver"))
                            {
                                configInfo.TurninGoal = TurninState.Silver;
                                C.SaveDebounced();
                            }

                        }
                        ImGui.SameLine();
                        using (ImRaii.PushColor(ImGuiCol.Text, bronzeEnabled ? BronzeColor : DisabledColor))
                        {
                            if (ImGuiEx.IconButton(FontAwesomeIcon.Trophy, "##Bronze"))
                            {
                                configInfo.TurninGoal = TurninState.Bronze;
                                C.SaveDebounced();
                            }
                        }
                    }

                    ImGui.PopID();
                }
            }
        }
        public sealed class PlanetColumn : ItemFilterColumn
        {
            public PlanetColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
                var moons = CosmicMoonRegistry.All;
                SetFlags(moons.Select(m => m.PlanetFilter).ToArray());
                SetNames(moons.Select(m => m.DisplayName).ToArray());
            }
            public override float Width => Math.Max(
                ImGui.CalcTextSize(Label + "X").X + ImGui.GetStyle().CellPadding.X * 2,
                ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2
            );

            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.TerritoryId.CompareTo(rhs.SheetInfo.TerritoryId);
            public override void DrawColumn(MissionInfo item, int idx)
            {
                var status = item.SheetInfo.CompletionStatus;
                var frameHeight = ImGui.GetFrameHeight();
                var size = new Vector2(frameHeight - 2);

                var columnWidth = ImGui.GetColumnWidth();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (columnWidth - frameHeight) / 2);

                var planetIcon = CosmicMoonRegistry.GetIconResource(item.SheetInfo.TerritoryId);

                var texture = Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), planetIcon).GetWrapOrEmpty();
                ImGui.Image(texture.Handle, size);
            }
            public override bool FilterFunc(MissionInfo item) =>
                CosmicMoonRegistry.ItemFilterIncludesTerritory(FilterValue, item.SheetInfo.TerritoryId);
        }
        public sealed class JobColumn : JobFilterColumn
        {
            public JobColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
                SetFlagsAndNames(JobFilter.CRP, JobFilter.BSM, JobFilter.ARM, JobFilter.GSM,
                                 JobFilter.LTW, JobFilter.WVR, JobFilter.ALC, JobFilter.CUL,
                                 JobFilter.MIN, JobFilter.BTN, JobFilter.FSH);
            }
            public override float Width => Math.Max(ImGui.CalcTextSize(Label + "XX").X + ImGui.GetStyle().CellPadding.X * 2, ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2);
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.Jobs.First().CompareTo(rhs.SheetInfo.Jobs.First());

            public override void DrawColumn(MissionInfo item, int idx)
            {
                var frameHeight = ImGui.GetFrameHeight();
                var size = new Vector2(frameHeight);
                var jobs = item.SheetInfo.Jobs;

                var tightSpacing = 2f; // adjust this if I don't like how close/far they are
                var totalWidth = frameHeight * jobs.Count + tightSpacing * (jobs.Count - 1);

                var columnWidth = ImGui.GetColumnWidth();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (columnWidth - totalWidth) / 2);

                using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(tightSpacing, 0));
                foreach (var job in jobs)
                {
                    var icon = CosmicHelper.ClassInfoDict[job].JobIcon;
                    ImGui.Image(icon.GetWrapOrEmpty().Handle, size);
                    ImGui.SameLine();
                }
            }
            public override bool FilterFunc(MissionInfo item)
            {
                return item.SheetInfo.Jobs.Any(job =>
                {
                    var flag = job switch
                    {
                        8 => JobFilter.CRP,
                        9 => JobFilter.BSM,
                        10 => JobFilter.ARM,
                        11 => JobFilter.GSM,
                        12 => JobFilter.LTW,
                        13 => JobFilter.WVR,
                        14 => JobFilter.ALC,
                        15 => JobFilter.CUL,
                        16 => JobFilter.MIN,
                        17 => JobFilter.BTN,
                        18 => JobFilter.FSH,
                        _ => JobFilter.None,
                    };
                    return FilterValue.HasFlag(flag);
                });
            }
        }
        public sealed class ProfileColumn : ItemFilterColumn
        {
            public ProfileColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
            }
            public override float Width => Math.Max(
                ImGui.CalcTextSize(Label).X + ImGui.GetStyle().CellPadding.X * 2,
                ImGui.CalcTextSize("Open Craft Settings").X + ImGui.GetStyle().FramePadding.X * 2 + ImGui.GetStyle().CellPadding.X * 2
            );
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.Jobs.First().CompareTo(rhs.SheetInfo.Jobs.First());
            public override void DrawColumn(MissionInfo item, int idx)
            {
                var sheetInfo = item.SheetInfo;
                bool craftProfile = sheetInfo.Attributes.HasFlag(MissionAttributes.Craft);
                bool gatherProfile = sheetInfo.Attributes.HasFlag(MissionAttributes.Gather) || sheetInfo.IsGreaterReach;
                bool collectable = sheetInfo.Attributes.HasFlag(MissionAttributes.Collectables) || sheetInfo.Attributes.HasFlag(MissionAttributes.ReducedItems);
                bool fishProfile = sheetInfo.Attributes.HasFlag(MissionAttributes.Fish);

                ImGui.PushID($"Mission: {item.Id}");


                if (sheetInfo.Attributes.HasFlag(MissionAttributes.Craft))
                {
                    if (ImGui.Button($"Open Craft Settings##Craft_{item.Id}"))
                    {
                        ImGui.OpenPopup("Craft Settings: Recipies");
                    }

                    if (ImGui.BeginPopup("Craft Settings: Recipies"))
                    {
                        ImGui.TextDisabled($"{item.Id}");
                        ImGui.SameLine();
                        ImGui.Text($"Mission: {sheetInfo.Name}");

                        CrafterManagement(sheetInfo, item.Id);

                        ImGui.EndPopup();
                    }
                }

                if (sheetInfo.Jobs.Count > 1)
                {
                    // ImGui.SameLine();
                }

                if (gatherProfile)
                {
                    if (!collectable)
                    {
                        string profileName = "???";
                        if (C.MissionConfig.TryGetValue(item.Id, out var config))
                        {
                            if (C.GatherProfiles.TryGetValue(config.GProfileId, out var profileSetting))
                            {
                                profileName = profileSetting.Name;
                            }

                            if (ImGui.Button($"{profileName}##{item.Id}_{item.SheetInfo.Name}"))
                            {
                                ImGui.OpenPopup($"Select Gather Profile");
                            }
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip("Select gathering profile");
                            }
                            if (ImGui.BeginPopup($"Select Gather Profile"))
                            {
                                ImGui.Text($"Mission: [{item.Id}] {item.SheetInfo.Name}");
                                ImGui.Text($"Currently Selected: {profileName}");
                                ImGui.Separator();

                                foreach (var profile in C.GatherProfiles)
                                {
                                    var id = profile.Key;
                                    bool profileSelected = config.GProfileId == id;
                                    ImGui.PushID($"{id}_{profile.Value.Name}");
                                    if (ImGui.RadioButton(profile.Value.Name, profileSelected))
                                    {
                                        config.GProfileId = id;
                                        C.Save();
                                    }
                                    ImGui.PopID();
                                }

                                ImGui.EndPopup();
                            }
                        }
                    }
                    else
                    {
                        ImGuiUtil.Center("Auto");
                    }
                }
                else if (sheetInfo.Attributes.HasFlag(MissionAttributes.Fish))
                {
                    if (C.MissionConfig.TryGetValue(item.Id, out var config))
                    {
                        if (ImGui.Button($"Fishing Settings"))
                        {
                            ImGui.OpenPopup("Select Fishing Profile");
                        }
                        if (ImGui.BeginPopup("Select Fishing Profile"))
                        {
                            ImGui.Text($"Fishing profile: {sheetInfo.Name}");
                            ImGui.Separator();
                            bool builtInPreset = config.Use_BuildinPreset;
                            if (ImGui.Checkbox("Use Built In Preset", ref builtInPreset))
                            {
                                config.Use_BuildinPreset = builtInPreset;
                                C.Save();
                            }
                            ImGuiEx.HelpMarker("Having this enabled means it will use the default preset that is included with the plugin for autohook. \n" +
                                               "If you would like to use one that you already have in autohook, you can un-checkmark this and type the name of it below");
                            using (ImRaii.Disabled(builtInPreset))
                            {
                                string presetName = config.AutoHookPresetName;
                                ImGui.SetNextItemWidth(200);
                                if (ImGui.InputText("Preset Name", ref presetName))
                                {
                                    config.AutoHookPresetName = presetName;
                                    C.Save();
                                }
                            }

                            ImGui.EndPopup();
                        }
                    }
                }
            }
        }
        public sealed class NotesColumn : ItemFilterColumn
        {
            public NotesColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
                SetFlags(ItemFilter.BestSPM, ItemFilter.Sequence, ItemFilter.Unlock, ItemFilter.NoNotes);
                SetNames("Best Score Per Minute", "Sequence", "Needs Unlocked", "No Notes");
            }
            public override float Width => Math.Max(
                ImGui.CalcTextSize(Label + "xxx").X + ImGui.GetStyle().CellPadding.X * 2,
                ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2
            );
            public override void DrawColumn(MissionInfo item, int idx)
            {
                var sheetInfo = item.SheetInfo;
                var HasSPM = sheetInfo.BestSPM.SPM > 0;
                var HasSequence = sheetInfo.SequenceMissions_Next.Count() > 0 || sheetInfo.SequenceMissions_Previous.Count() > 0;
                var HasUnlockable = sheetInfo.MissionUnlock.Count() > 0;

                if (HasSPM)
                {
                    ImGuiEx.Icon(FontAwesomeIcon.Trophy);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text($"Average SPM: {sheetInfo.BestSPM.SPM:N2}");
                        ImGui.Text($"{sheetInfo.BestSPM.NoteInfo}");
                        ImGui.EndTooltip();
                    }
                }
                if (HasSequence)
                {
                    if (HasSPM)
                        ImGui.SameLine();

                    ImGuiEx.Icon(FontAwesomeIcon.ListOl);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        if (sheetInfo.SequenceMissions_Next.Count() > 0)
                        {
                            ImGui.Text("Next Sequence:");
                            foreach(var mission in sheetInfo.SequenceMissions_Next)
                            {
                                var seqInfo = CosmicHelper.SheetMissionDict[mission];
                                ImGui.Text($"[{mission}] {seqInfo.Name}");
                            }
                        }
                        if (sheetInfo.SequenceMissions_Previous.Count() > 0)
                        {
                            ImGui.Text("Previous Sequence:");
                            foreach (var mission in sheetInfo.SequenceMissions_Previous)
                            {
                                var seqInfo = CosmicHelper.SheetMissionDict[mission];
                                ImGui.Text($"[{mission}] {seqInfo.Name}");
                            }
                        }
                        ImGui.EndTooltip();
                    }
                }
                if (HasUnlockable)
                {
                    if (HasSPM || HasSequence)
                    {
                        ImGui.SameLine();
                    }
                    if (Svc.Texture.GetFromGame("ui/uld/WKSMission_hr1.tex") is { } tex)
                    {
                        var frameHeight = ImGui.GetFrameHeight();
                        var size = new Vector2(frameHeight);
                        if (tex.TryGetWrap(out var wrap, out var exc))
                        {
                            ImGui.Image(wrap.Handle, size, new Vector2(0.2347f, 0.3500f), new Vector2(0.2959f, 0.6500f));
                        }
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text("The following missions are required to have gold before you can do this one");
                        foreach (var mission in sheetInfo.MissionUnlock)
                        {
                            ImGui_Ice.CompletionStatusIcon(CosmicHelper.SheetMissionDict[mission]);
                            ImGui.SameLine();
                            ImGui.Text($"[{mission}] - {CosmicHelper.SheetMissionDict[mission].Name}");
                        }
                        ImGui.EndTooltip();
                    }
                }
            }
        }
        public static void CrafterManagement(CosmicHelper.CosmicInfo mission, uint id, ImGuiTreeNodeFlags openDefault = ImGuiTreeNodeFlags.DefaultOpen)
        {
            var job = mission.Jobs.First(x => CosmicHelper.CrafterJobList.Contains(x));
            ImGui.Text("Recipe Detailed Info");

            Dictionary<ushort, CosmicHelper.CraftingInfo> missionCrafts = new();
            foreach (var craft in mission.Crafts_Main)
                missionCrafts[craft.Key] = craft.Value;
            foreach (var craft in mission.Crafts_Pre)
                missionCrafts[craft.Key] = craft.Value;

            bool massApplyButton = ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift);

            if (ImGui.CollapsingHeader("Craft Item Settings", openDefault))
            {
                using (ImRaii.Disabled(!massApplyButton))
                {
                    ImGui.PushID(id);

                    if (ImGui.Button("Apply to similar missions"))
                    {
                        var currentMission = CosmicHelper.SheetMissionDict[id];
                        var recipeConfig = C.MissionConfig[id];

                        var currentRecipeSettings = new Dictionary<(int, int, int), MissionSettings.ArtisanSettings>();

                        foreach (var (key, craft) in currentMission.Crafts_Main)
                            if (recipeConfig.CraftSettings.TryGetValue(key, out var settings))
                                currentRecipeSettings[(craft.RecipeInfo.Durability, craft.RecipeInfo.Progress, craft.RecipeInfo.Quality)] = settings;

                        foreach (var (key, craft) in currentMission.Crafts_Pre)
                            if (recipeConfig.CraftSettings.TryGetValue(key, out var settings))
                                currentRecipeSettings[(craft.RecipeInfo.Durability, craft.RecipeInfo.Progress, craft.RecipeInfo.Quality)] = settings;

                        int appliedMissions = 0;
                        int appliedCrafts = 0;

                        void ApplyMatchingCrafts(Dictionary<ushort, CosmicHelper.CraftingInfo> crafts, MissionSettings targetConfig, ref int craftCount)
                        {
                            foreach (var (key, craft) in crafts)
                            {
                                var recipeKey = (craft.RecipeInfo.Durability, craft.RecipeInfo.Progress, craft.RecipeInfo.Quality);
                                if (!currentRecipeSettings.TryGetValue(recipeKey, out var src))
                                    continue;

                                targetConfig.CraftSettings[key] = new MissionSettings.ArtisanSettings
                                {
                                    UseGlobal = src.UseGlobal,
                                    FoodId = src.FoodId,
                                    FoodHQ = src.FoodHQ,
                                    PotionId = src.PotionId,
                                    PotionHQ = src.PotionHQ,
                                    ManualId = src.ManualId,
                                    SquadronManualId = src.SquadronManualId,
                                    ArtisanSolverType = src.ArtisanSolverType,
                                    MacroName = src.MacroName,
                                    SkillUsageAmount = src.SkillUsageAmount,
                                    MinStepsForMiracle = src.MinStepsForMiracle,
                                    ExpertProfileId = src.ExpertProfileId,
                                };
                                craftCount++;
                            }
                        }

                        foreach (var sheetMission in CosmicHelper.SheetMissionDict)
                        {
                            if (!sheetMission.Value.Attributes.HasFlag(MissionAttributes.Craft))
                                continue;

                            if (sheetMission.Key == id)
                                continue;

                            if (!C.MissionConfig.TryGetValue(sheetMission.Key, out var targetConfig))
                            {
                                targetConfig = new MissionSettings();
                                C.MissionConfig[sheetMission.Key] = targetConfig;
                            }

                            int craftsBeforeApply = appliedCrafts;
                            ApplyMatchingCrafts(sheetMission.Value.Crafts_Main, targetConfig, ref appliedCrafts);
                            ApplyMatchingCrafts(sheetMission.Value.Crafts_Pre, targetConfig, ref appliedCrafts);

                            if (appliedCrafts > craftsBeforeApply)
                                appliedMissions++;
                        }

                        IceLogging.Info($"Amount of missions applied to: {appliedMissions}\n" +
                            $"Total amount of crafts applied to: {appliedCrafts}\n" +
                            $"Amount of recipies that the mission had: {currentRecipeSettings.Count()}\n" +
                            $"From Mission: {id}");
                    }

                    ImGui.PopID();
                }
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled) && !massApplyButton)
                {
                    ImGui.SetTooltip("Hold shift to allow applying");
                }

                foreach (var craft in missionCrafts)
                {
                    if (ImGui.BeginTable($"Main Craft Details_{craft.Key}", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Hideable))
                    {
                        ImGui.TableSetupColumn("Item Details");
                        ImGui.TableSetupColumn("Dropdown Detail");
                        ImGui.TableSetupColumn("Dropdown Selection", ImGuiTableColumnFlags.WidthStretch);

                        if (C.MissionConfig[id].CraftSettings.TryGetValue(craft.Key, out var recipeConfig))
                        {
                            bool globalArtisan = recipeConfig.UseGlobal;
                            bool supportedArtisan = P.Artisan.UpdatedArtisan();

                            ImGui.TableSetColumnEnabled(1, !globalArtisan);
                            ImGui.TableSetColumnEnabled(2, !globalArtisan);

                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            if (ImGui.Checkbox("Use Global Artisan Settings", ref globalArtisan))
                            {
                                recipeConfig.UseGlobal = globalArtisan;
                                C.Save();
                            }

                            #region Label info

                            string GetSolverLabel(ArtisanCraftType type)
                            {
                                return type switch
                                {
                                    ArtisanCraftType.Default => "Default",
                                    ArtisanCraftType.Raphael => "Raphael Solver",
                                    ArtisanCraftType.ProgressOnly => "Progress Only Solver",
                                    ArtisanCraftType.Standard => "Standard Solver",
                                    ArtisanCraftType.Expert => "Expert Recipe Solver",
                                    ArtisanCraftType.Macro => "Artisan Macro",
                                    _ => "Unknown"
                                };
                            }
                            string GetFoodLable(uint foodId)
                            {
                                if (foodId == 0) return "Default";
                                var item = ConsumableInfo.CrafterFood.FirstOrDefault(x => x.Id == foodId);
                                PlayerHelper.GetItemCount(item.Id, out var nq, includeHq: false, includeNq: true);
                                PlayerHelper.GetItemCount(item.Id, out var hq, includeHq: true, includeNq: false);
                                return BuildItemLabel(item.Name, nq, hq);
                            }
                            string GetPotionLable(uint potionId)
                            {
                                if (potionId == 0) return "Default";
                                var item = ConsumableInfo.Pots.FirstOrDefault(x => x.Id == potionId);
                                PlayerHelper.GetItemCount(item.Id, out var nq, includeHq: false, includeNq: true);
                                PlayerHelper.GetItemCount(item.Id, out var hq, includeHq: true, includeNq: false);
                                return BuildItemLabel(item.Name, nq, hq);
                            }
                            string GetManualLabel(uint manualId)
                            {
                                if (manualId == 0) return "Default";
                                var item = ConsumableInfo.Manuals.FirstOrDefault(x => x.Id == manualId);
                                PlayerHelper.GetItemCount(item.Id, out var nq, includeHq: false, includeNq: true);
                                return BuildItemLabel(item.Name, nq, 0);
                            }
                            string GetSquadronManualLabel(uint squadManualId)
                            {
                                if (squadManualId == 0) return "Default";
                                var item = ConsumableInfo.SquadronManuals.FirstOrDefault(x => x.Id == squadManualId);
                                PlayerHelper.GetItemCount(item.Id, out var nq, includeHq: false, includeNq: true);
                                return BuildItemLabel(item.Name, nq, 0);
                            }
                            string BuildItemLabel(string name, int nqCount, int hqCount)
                            {
                                var parts = new List<string>();
                                if (hqCount > 0) parts.Add($"{(char)0xE03C} {name} [x{hqCount}]");
                                if (nqCount > 0) parts.Add($"{name} [x{nqCount}]");
                                return string.Join(" / ", parts);
                            }

                            var recipe_Solver = GetSolverLabel(recipeConfig.ArtisanSolverType);
                            var recipe_FoodLabel = GetFoodLable(recipeConfig.FoodId);
                            var recipe_PotionLabel = GetPotionLable(recipeConfig.PotionId);
                            var recipe_ManualLabel = GetManualLabel(recipeConfig.ManualId);
                            var recipe_SquadManualLabel = GetSquadronManualLabel(recipeConfig.SquadronManualId);

                            float recipe_ComboWidth = new[]
                            {
                                                recipe_FoodLabel,
                                                recipe_PotionLabel,
                                                recipe_ManualLabel,
                                                recipe_SquadManualLabel,
                                                recipe_Solver
                                            }.Max(label => ImGui.CalcTextSize(label).X + ImGui.GetStyle().FramePadding.X * 2 + ImGui.GetStyle().ScrollbarSize + 10);

                            List<ArtisanCraftType> standardSolvers = new()
                                    {
                                        ArtisanCraftType.Default,
                                        ArtisanCraftType.Standard,
                                        ArtisanCraftType.Raphael,
                                        ArtisanCraftType.ProgressOnly,
                                        ArtisanCraftType.Macro,
                                    };

                            List<ArtisanCraftType> expertSolvers = new()
                                    {
                                        ArtisanCraftType.Default,
                                        ArtisanCraftType.Expert,
                                        ArtisanCraftType.Raphael,
                                        ArtisanCraftType.Macro,
                                    };

                            #endregion

                            #region Image

                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            if (Svc.Texture.TryGetFromGameIcon(craft.Value.IconId, out var iconImage))
                            {
                                ImGui.Image(iconImage.GetWrapOrEmpty().Handle, new Vector2(24, 24));
                            }
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text($"Key / RecipeId: {craft.Key}");
                                ImGui.Text($"ItemID: {craft.Value.ItemId}");
                                ImGui.EndTooltip();
                            }
                            if (craft.Value.ExpertCraft)
                            {
                                ImGui.SameLine();
                                ImGui.AlignTextToFramePadding();
                                ImGuiEx.Icon(new Vector4(1.0f, 0.4f, 0.0f, 1.0f), FontAwesomeIcon.Diamond);
                                if (ImGui.IsItemHovered())
                                {
                                    ImGui.SetTooltip("Expert Craft");
                                }
                            }

                            #endregion

                            #region Item Name + Solver


                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            ImGui.AlignTextToFramePadding();
                            ImGui.Text($"{craft.Value.ItemName}");

                            ImGui.TableNextColumn();
                            ImGui.Text("Solver");

                            ImGui.TableNextColumn();
                            ImGui.SetNextItemWidth(recipe_ComboWidth);
                            if (ImGui.BeginCombo("##Solver", recipe_Solver))
                            {
                                if (craft.Value.ExpertCraft)
                                {
                                    foreach (var type in expertSolvers)
                                    {
                                        bool isSelected = recipeConfig.ArtisanSolverType == type;
                                        if (ImGui.Selectable(GetSolverLabel(type), isSelected))
                                        {
                                            recipeConfig.ArtisanSolverType = type;
                                            C.Save();
                                        }
                                        if (isSelected)
                                            ImGui.SetItemDefaultFocus();
                                    }
                                }
                                else
                                {
                                    foreach (var type in standardSolvers)
                                    {
                                        bool isSelected = recipeConfig.ArtisanSolverType == type;
                                        if (ImGui.Selectable(GetSolverLabel(type), isSelected))
                                        {
                                            recipeConfig.ArtisanSolverType = type;
                                            C.Save();
                                        }
                                        if (isSelected)
                                            ImGui.SetItemDefaultFocus();
                                    }
                                }

                                ImGui.EndCombo();
                            }

                            if (recipeConfig.ArtisanSolverType == ArtisanCraftType.Macro)
                            {
                                string macroName = recipeConfig.MacroName;
                                ImGui.SameLine();
                                ImGui.SetNextItemWidth(200);
                                if (ImGui.InputText("Macro Name", ref macroName))
                                {
                                    recipeConfig.MacroName = macroName;
                                    C.Save();
                                }
                            }

                            #endregion

                            #region Durability + Food

                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            ImGui.AlignTextToFramePadding();
                            ImGui.Text($"Durability: {craft.Value.RecipeInfo.Durability}");

                            if (supportedArtisan)
                            {
                                ImGui.TableNextColumn();
                                ImGui.Text("Food");

                                ImGui.TableNextColumn();
                                ImGui.SetNextItemWidth(recipe_ComboWidth);
                                if (ImGui.BeginCombo("##FoodSelection", recipe_FoodLabel))
                                {
                                    bool isDefaultSelected = recipeConfig.FoodId == 0;
                                    if (ImGui.Selectable("Default", isDefaultSelected))
                                    {
                                        recipeConfig.FoodId = 0;
                                        recipeConfig.FoodHQ = false;
                                        C.Save();
                                    }
                                    if (isDefaultSelected)
                                        ImGui.SetItemDefaultFocus();

                                    ImGui.Separator();

                                    foreach (var item in ConsumableInfo.CrafterFood)
                                    {
                                        PlayerHelper.GetItemCount(item.Id, out var nqCount, includeHq: false, includeNq: true);
                                        PlayerHelper.GetItemCount(item.Id, out var hqCount, includeHq: true, includeNq: false);

                                        if (nqCount == 0 && hqCount == 0) continue;

                                        bool isSelected = recipeConfig.FoodId == item.Id;
                                        string label = BuildItemLabel(item.Name, nqCount, hqCount) + $"###{item.Id}";

                                        if (ImGui.Selectable(label, isSelected))
                                        {
                                            recipeConfig.FoodId = item.Id;
                                            recipeConfig.FoodHQ = hqCount > 0;
                                            C.Save();
                                        }

                                        if (isSelected)
                                            ImGui.SetItemDefaultFocus();
                                    }

                                    ImGui.EndCombo();
                                }
                            }

                            #endregion

                            #region Progress + Potion

                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            ImGui.AlignTextToFramePadding();
                            ImGui.Text($"Progress: {craft.Value.RecipeInfo.Progress}");

                            if (supportedArtisan)
                            {
                                ImGui.TableNextColumn();
                                ImGui.Text("Potion");

                                ImGui.TableNextColumn();
                                ImGui.SetNextItemWidth(recipe_ComboWidth);
                                if (ImGui.BeginCombo("##StandardPotion", recipe_PotionLabel))
                                {
                                    // Default option
                                    bool isDefaultSelected = recipeConfig.PotionId == 0;
                                    if (ImGui.Selectable("Default", isDefaultSelected))
                                    {
                                        recipeConfig.PotionId = 0;
                                        recipeConfig.PotionHQ = false;
                                        C.Save();
                                    }
                                    if (isDefaultSelected)
                                        ImGui.SetItemDefaultFocus();

                                    ImGui.Separator();

                                    foreach (var item in ConsumableInfo.Pots)
                                    {
                                        PlayerHelper.GetItemCount(item.Id, out var nqCount, includeHq: false, includeNq: true);
                                        PlayerHelper.GetItemCount(item.Id, out var hqCount, includeHq: true, includeNq: false);

                                        if (nqCount == 0 && hqCount == 0) continue;

                                        bool isSelected = recipeConfig.PotionId == item.Id;
                                        string label = BuildItemLabel(item.Name, nqCount, hqCount) + $"###{item.Id}";

                                        if (ImGui.Selectable(label, isSelected))
                                        {
                                            recipeConfig.PotionId = item.Id;
                                            recipeConfig.PotionHQ = hqCount > 0;
                                            C.Save();
                                        }

                                        if (isSelected)
                                            ImGui.SetItemDefaultFocus();
                                    }

                                    ImGui.EndCombo();
                                }
                            }

                            #endregion

                            #region Quality + Manual

                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            ImGui.AlignTextToFramePadding();
                            ImGui.Text($"Quality: {craft.Value.RecipeInfo.Quality}");

                            if (supportedArtisan)
                            {
                                ImGui.TableNextColumn();
                                ImGui.AlignTextToFramePadding();
                                ImGui.Text("Manual");

                                ImGui.TableNextColumn();
                                ImGui.SetNextItemWidth(recipe_ComboWidth);
                                if (ImGui.BeginCombo("##StandardManual", recipe_ManualLabel))
                                {
                                    // Default option
                                    bool isDefaultSelected = recipeConfig.ManualId == 0;
                                    if (ImGui.Selectable("Default", isDefaultSelected))
                                    {
                                        recipeConfig.ManualId = 0;
                                        C.Save();
                                    }
                                    if (isDefaultSelected)
                                        ImGui.SetItemDefaultFocus();

                                    ImGui.Separator();

                                    foreach (var item in ConsumableInfo.Manuals)
                                    {
                                        PlayerHelper.GetItemCount(item.Id, out var nqCount, includeHq: false, includeNq: true);

                                        if (nqCount == 0) continue;

                                        bool isSelected = recipeConfig.ManualId == item.Id;
                                        string label = BuildItemLabel(item.Name, nqCount, 0) + $"###{item.Id}";

                                        if (ImGui.Selectable(label, isSelected))
                                        {
                                            recipeConfig.ManualId = item.Id;
                                            C.Save();
                                        }

                                        if (isSelected)
                                            ImGui.SetItemDefaultFocus();
                                    }

                                    ImGui.EndCombo();
                                }
                            }

                            #endregion

                            #region Squadron Manual

                            if (!globalArtisan)
                            {
                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(1);
                                ImGui.Text("Squadron Manual");

                                ImGui.TableNextColumn();
                                ImGui.SetNextItemWidth(recipe_ComboWidth);
                                if (ImGui.BeginCombo("##StandardSquadManual", recipe_SquadManualLabel))
                                {
                                    // Default option
                                    bool isDefaultSelected = recipeConfig.SquadronManualId == 0;
                                    if (ImGui.Selectable("Default", isDefaultSelected))
                                    {
                                        recipeConfig.SquadronManualId = 0;
                                        C.Save();
                                    }
                                    if (isDefaultSelected)
                                        ImGui.SetItemDefaultFocus();

                                    ImGui.Separator();

                                    foreach (var item in ConsumableInfo.SquadronManuals)
                                    {
                                        PlayerHelper.GetItemCount(item.Id, out var nqCount, includeHq: false, includeNq: true);

                                        if (nqCount == 0) continue;

                                        bool isSelected = recipeConfig.SquadronManualId == item.Id;
                                        string label = BuildItemLabel(item.Name, nqCount, 0) + $"###{item.Id}";

                                        if (ImGui.Selectable(label, isSelected))
                                        {
                                            recipeConfig.SquadronManualId = item.Id;
                                            C.Save();
                                        }

                                        if (isSelected)
                                            ImGui.SetItemDefaultFocus();
                                    }

                                    ImGui.EndCombo();
                                }
                            }

                            #endregion

                            #region ActionUsage

                            if (mission.TemporaryActionCount != 0)
                            {
                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(0);
                                var actionInfo = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>().GetRow(mission.TemporaryActionId);
                                var name = actionInfo.Name;
                                var icon = Svc.Texture.GetFromGameIcon((int)actionInfo.Icon).GetWrapOrEmpty();
                                ImGui.Image(icon.Handle, new(24, 24));
                                ImGui.AlignTextToFramePadding();
                                ImGui.SameLine();
                                ImGui.Text($"{name}");

                                if (supportedArtisan)
                                {
                                    ImGui.TableNextColumn();
                                    ImGui.Text($"Max use");

                                    ImGui.TableNextColumn();
                                    var maxUsage = recipeConfig.SkillUsageAmount;
                                    ImGui.SetNextItemWidth(recipe_ComboWidth);
                                    string skillUsageLabel = maxUsage == -1 ? "Default" : $"{maxUsage}";
                                    if (ImGui.SliderInt("##MaxSkillUsage", ref maxUsage, -1, (int)mission.TemporaryActionCount, skillUsageLabel))
                                    {
                                        recipeConfig.SkillUsageAmount = maxUsage;
                                        C.SaveDebounced();
                                    }

                                    ImGui.TableNextRow();
                                    ImGui.TableSetColumnIndex(0);
#if DEBUG
                                    if (ImGui.Button("Test Apply"))
                                    {
                                        var key = craft.Key;
                                        var useAmount = recipeConfig.SkillUsageAmount;
                                        var miracleSteps = recipeConfig.MinStepsForMiracle;
                                        IceLogging.Verbose($"Was Expert: {craft.Value.ExpertCraft}", "Test Apply Skills");
                                        if (craft.Value.ExpertCraft)
                                        {
                                            if (recipeConfig.SkillUsageAmount != -1)
                                            {
                                                P.Artisan.ChangeExpertMaxSteadyUses(key, (uint)useAmount, true);
                                                P.Artisan.ChangeExpertMaxMaterialMiracleUses(key, (uint)useAmount, true);
                                            }
                                            else
                                            {
                                                P.Artisan.SetTempExpertMaxSteadyUsesBackToNormal(key);
                                                P.Artisan.SetTempExpertMaxMaterialMiracleUsesBackToNormal(key);
                                            }

                                            if (miracleSteps != -1)
                                                P.Artisan.ChangeExpertMinimumStepsBeforeMiracle(key, (uint)miracleSteps, true);
                                            else
                                                P.Artisan.SetTempExpertMinimumStepsBeforeMiracleBackToNormal(key);
                                        }
                                        else
                                        {
                                            if (useAmount != -1)
                                            {
                                                P.Artisan.ChangeStandardMaxMaterialMiracleUses((uint)useAmount, true);
                                            }
                                            else
                                            {
                                                P.Artisan.SetTempStandardMaxMaterialMiracleUsesBackToNormal();
                                            }

                                            if (miracleSteps != 1)
                                            {
                                                P.Artisan.ChangeStandardMinimumStepsBeforeMiracle((uint)miracleSteps, true);
                                            }
                                            else
                                            {
                                                P.Artisan.SetTempStandardMinimumStepsBeforeMiracleBackToNormal();
                                            }
                                        }
                                    }
#endif

                                    if (mission.TemporaryActionId == 41269 && !globalArtisan)
                                    {
                                        ImGui.TableSetColumnIndex(1);
                                        ImGui.Text("Use after this many steps");

                                        ImGui.TableNextColumn();
                                        var minSteps = recipeConfig.MinStepsForMiracle;
                                        string skillMinStepsName = minSteps == -1 ? "Default" : $"{minSteps}";
                                        ImGui.SetNextItemWidth(recipe_ComboWidth);
                                        if (ImGui.SliderInt("##MinMiracleSteps", ref minSteps, -1, 20, skillMinStepsName))
                                        {
                                            recipeConfig.MinStepsForMiracle = minSteps;
                                            C.SaveDebounced();
                                        }
                                    }
                                }
                            }

                            #endregion
                        }
                        else
                        {
                            C.MissionConfig[id].CraftSettings[craft.Key] = new();
                            C.SaveDebounced();
                        }

                        ImGui.EndTable();
                    }
                }
            }
        }
        private static ItemFilter TierToFlag(int tier) => tier switch
        {
            1 => ItemFilter.HasI,
            2 => ItemFilter.HasII,
            3 => ItemFilter.HasIII,
            4 => ItemFilter.HasIV,
            5 => ItemFilter.HasV,
            6 => ItemFilter.HasVI,
            7 => ItemFilter.HasVII,
            _ => ItemFilter.HasI
        };
    }
}
