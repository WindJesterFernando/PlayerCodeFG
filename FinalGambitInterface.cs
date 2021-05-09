using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace Final_Gambit_Player_Code
{

    public class PartyAIManager
    {

        const int CoreStats = 1;
        const int Perk = 2;
        const int StatusEffect = 3;
        const int PassiveAbility = 4;
        const int PartyInventoryItem = 5;
        const int Ability = 6;
        const int FoePartyInfoStart = 7;

        const int CharacterWithInitiative = 8;
        const int PossibleAction = 9;
        const int ActionLegality = 10;
        const int RecordOfBattleAction = 11;
        const int IsFistStateSentForNewBattle = 12;

        public static bool actionFileHasBeenWriten;

        public PartyAIManager()
        {
            FileNames.Init();
            FinalGambit.BattleActionID.Init();
            FinalGambit.CharacterClassID.Init();
            FinalGambit.PassiveAbilityID.Init();
            FinalGambit.StatusEffectID.Init();
            FinalGambit.ItemID.Init();
            FinalGambit.BattleActionID.Init();
            FinalGambit.ItemCost.Init();
            FinalGambit.BattleActionMPCost.Init();
            FinalGambit.CraftMaterialCost.Init();
            FinalGambit.recordOfBattleActions = new LinkedList<RecordOfBattleAction>();


            if (File.Exists(FileNames.battleReport) && File.Exists(FileNames.flag))
            {
                if (!File.Exists(FileNames.battleReport))
                {
                    FileStream f = File.Open(FileNames.battleReport, FileMode.Open);
                    StreamReader sr = new StreamReader(f);

                    string data;
                    while ((data = sr.ReadLine()) != null)
                    {
                        string[] processing = data.Split(",");
                        int identifier = Int32.Parse(processing[0]);

                        FinalGambit.recordOfBattleActions.AddLast(new RecordOfBattleAction(Int32.Parse(processing[1]), Int32.Parse(processing[2]), Int32.Parse(processing[3]), Int32.Parse(processing[4])));
                    }
                    sr.Close();
                    f.Close();
                }
            }

            while (true)
            {

                if (File.Exists(FileNames.error) && File.Exists(FileNames.flag))
                {
                    FileStream f = File.Open(FileNames.error, FileMode.Open);
                    StreamReader sr = new StreamReader(f);

                    string data;
                    while ((data = sr.ReadLine()) != null)
                    {
                        Console.WriteLine("Error: " + data);
                    }

                    sr.Close();
                    f.Close();

                    File.Delete(FileNames.error);

                    Console.WriteLine("Press any key to continue.");
                    Console.ReadKey();


                }
                if (File.Exists(FileNames.battleState) && File.Exists(FileNames.flag))
                {
                    ReadBattleStateSerializationFile();
                    File.Delete(FileNames.battleState);
                    File.Delete(FileNames.flag);

                    PartyAIManager.actionFileHasBeenWriten = false;
                    PartyAI.ProcessAI();
                }

                Thread.Sleep(1000);
            }
        }

        private void ReadBattleStateSerializationFile()
        {

            FinalGambit.BattleState = new BattleState();


            FileStream f = File.OpenRead(FileNames.battleState);

            StreamReader sr = new StreamReader(f);

            bool isReadingForParty = true;
            PartyCharacter lastPC = null;
            LegalActionAndRequirments lastAction = null;

            // Console.WriteLine("starting reading ");

            string data;
            while ((data = sr.ReadLine()) != null)
            {
                //Console.WriteLine("reading " + data);
                string[] processing = data.Split(",");
                int identifier = Int32.Parse(processing[0]);

                if (identifier == CoreStats)
                {
                    lastPC = new PartyCharacter(Int32.Parse(processing[1]), Int32.Parse(processing[2]), Int32.Parse(processing[3]), Int32.Parse(processing[4]), Int32.Parse(processing[5]), float.Parse(processing[6]), float.Parse(processing[7]), Int32.Parse(processing[8]), Int32.Parse(processing[9]), Int32.Parse(processing[10]), Int32.Parse(processing[11]));
                    if (isReadingForParty)
                        FinalGambit.BattleState.partyCharacters.AddLast(lastPC);
                    else
                        FinalGambit.BattleState.foeCharacters.AddLast(lastPC);
                }
                else if (identifier == FoePartyInfoStart)
                {
                    isReadingForParty = false;
                }
                else if (identifier == Perk)
                {
                    lastPC.perkIDs.AddLast(Int32.Parse(processing[1]));
                }
                else if (identifier == StatusEffect)
                {
                    lastPC.statusEffects.AddLast(new StatusEffect(Int32.Parse(processing[1]), Int32.Parse(processing[2])));
                }
                else if (identifier == PassiveAbility)
                {
                    lastPC.passiveAbilities.AddLast(Int32.Parse(processing[1]));
                }
                else if (identifier == Ability)
                {
                    lastPC.battleActions.AddLast(Int32.Parse(processing[1]));
                }
                else if (identifier == PartyInventoryItem)
                {
                    if (isReadingForParty)
                        FinalGambit.BattleState.partyInventory.AddLast(new InventoryItem(Int32.Parse(processing[1]), Int32.Parse(processing[2])));
                    else
                        FinalGambit.BattleState.foeInventory.AddLast(new InventoryItem(Int32.Parse(processing[1]), Int32.Parse(processing[2])));
                }
                else if (identifier == CharacterWithInitiative)
                {
                    FinalGambit.BattleState.characterWithInitiative = FinalGambit.GetCharacterFromCharacterIndex(Int32.Parse(processing[1]));
                }
                else if (identifier == PossibleAction)
                {
                    lastAction = new LegalActionAndRequirments(Int32.Parse(processing[1]));
                    FinalGambit.BattleState.legalActions.AddLast(lastAction);
                }
                else if (identifier == ActionLegality)
                {
                    //Console.WriteLine("Adding legality " + Int32.Parse(processing[1]) + " to " + FinalGambit.BattleActionID.lookUp[lastAction.actionID]);
                    lastAction.legalities.AddLast(Int32.Parse(processing[1]));
                }
                else if (identifier == RecordOfBattleAction)
                {
                    FinalGambit.recordOfBattleActions.AddLast(new RecordOfBattleAction(Int32.Parse(processing[1]), Int32.Parse(processing[2]), Int32.Parse(processing[3]), Int32.Parse(processing[4])));

                    FileStream f2;
                    if (!File.Exists(FileNames.battleReport))
                    {
                        //Console.WriteLine("report create");
                        f2 = File.Create(FileNames.battleReport);
                    }
                    else
                    {
                        //Console.WriteLine("report append");
                        f2 = File.Open(FileNames.battleReport, FileMode.Append);
                    }

                    StreamWriter sw = new StreamWriter(f2);

                    //Console.WriteLine("report write = " + data);
                    sw.WriteLine("" + data);
                    sw.Close();

                    f2.Close();

                }
                else if (identifier == IsFistStateSentForNewBattle)
                {
                    FinalGambit.recordOfBattleActions = new LinkedList<RecordOfBattleAction>();
                    File.Delete(FileNames.battleReport);

                }

            }
            sr.Close();

            f.Close();
        }

    }

    public class BattleState
    {
        public LinkedList<PartyCharacter> partyCharacters;
        public LinkedList<PartyCharacter> foeCharacters;

        public LinkedList<InventoryItem> partyInventory;
        public LinkedList<InventoryItem> foeInventory;

        public PartyCharacter characterWithInitiative;
        public LinkedList<LegalActionAndRequirments> legalActions;


        public BattleState()
        {
            partyCharacters = new LinkedList<PartyCharacter>();
            foeCharacters = new LinkedList<PartyCharacter>();
            partyInventory = new LinkedList<InventoryItem>();
            foeInventory = new LinkedList<InventoryItem>();

            legalActions = new LinkedList<LegalActionAndRequirments>();
            characterWithInitiative = null;
        }
    }

    public class PartyCharacter
    {
        public int classID;

        public int hp, maxHP, mp, maxMP, defense, magicDefense, strength, wisdom;
        public float activeTimeBattleBarFill, speed;

        public LinkedList<int> battleActions;

        public LinkedList<int> perkIDs;
        public LinkedList<StatusEffect> statusEffects;

        public LinkedList<int> passiveAbilities;

        public PartyCharacter(int ClassID, int HP, int MaxHP, int MP, int MaxMP, float ActiveTimeBattleBarFill, float Speed, int Defense, int MagicDefense, int Attack, int MagicAttack)
        {
            classID = ClassID;

            hp = HP;
            mp = MP;
            maxHP = MaxHP;
            maxMP = MaxMP;
            activeTimeBattleBarFill = ActiveTimeBattleBarFill;
            speed = Speed;
            defense = Defense;
            magicDefense = MagicDefense;
            strength = Attack;
            wisdom = MagicAttack;

            battleActions = new LinkedList<int>();
            perkIDs = new LinkedList<int>();
            passiveAbilities = new LinkedList<int>();
            statusEffects = new LinkedList<StatusEffect>();
        }
    }

    public class InventoryItem
    {
        public int id;
        public int count;

        public InventoryItem(int ID, int Count)
        {
            id = ID;
            count = Count;
        }

    }

    public class StatusEffect
    {
        public int id;
        public int duration;

        public StatusEffect(int ID, int Duration)
        {
            id = ID;
            duration = Duration;
        }
    }

    public class LegalActionAndRequirments
    {
        public int actionID;
        public LinkedList<int> legalities;

        public LegalActionAndRequirments(int ActionID)
        {
            actionID = ActionID;
            legalities = new LinkedList<int>();
        }
    }

    static public class LegalTargetIdentifiers
    {
        public const int NoTargetRequired = 1;
        public const int TargetAlly = 2;
        public const int TargetFoe = 3;
        public const int TargetSlain = 4;

    }

    public class RecordOfBattleAction
    {
        public PartyCharacter characterWithInitiative;
        public PartyCharacter characterBeingTargeted;
        public PartyCharacter characterThatPerformedCover;
        public int battleActionID;

        public RecordOfBattleAction(int CharacterWithInitiative, int CharacterBeingTargeted, int CharacterThatPerformedCover, int BattleActionID)
        {
            characterWithInitiative = FinalGambit.GetCharacterFromCharacterIndex(CharacterWithInitiative);
            characterBeingTargeted = FinalGambit.GetCharacterFromCharacterIndex(CharacterBeingTargeted);
            characterThatPerformedCover = FinalGambit.GetCharacterFromCharacterIndex(CharacterThatPerformedCover);
            battleActionID = BattleActionID;
        }
    }

    public static class FinalGambit
    {
        public static BattleState BattleState;
        public static LinkedList<RecordOfBattleAction> recordOfBattleActions;

        static public void PerformBattleAction(int battleActionID, PartyCharacter target)
        {

            if (PartyAIManager.actionFileHasBeenWriten)
            {
                Console.WriteLine("PerformBattleAction has already been called!");
                return;
            }

            //if (AreBattleActionAndTargetLegal(battleActionID, target))
            {
                FileStream f = File.Create(FileNames.action);

                StreamWriter sw = new StreamWriter(f);

                if (target == null)
                    Console.WriteLine("Target is null!");

                int targetIndex = GetCharacterIndexFromCharacter(target);

                //Console.WriteLine("writing " + battleActionID + "," + targetIndex);

                sw.WriteLine(battleActionID + "," + targetIndex);

                sw.Close();
                f.Close();
                f = File.Create(FileNames.flag);
                f.Close();

                PartyAIManager.actionFileHasBeenWriten = true;
            }
        }

        static public int GetCharacterIndexFromCharacter(PartyCharacter pc)
        {
            int count = 0;
            if (FinalGambit.BattleState.foeCharacters.Contains(pc))
            {
                foreach (PartyCharacter pc2 in FinalGambit.BattleState.foeCharacters)
                {
                    count++;
                    if (pc2 == pc)
                        return -count;
                }
            }
            else if (FinalGambit.BattleState.partyCharacters.Contains(pc))
            {
                foreach (PartyCharacter pc2 in FinalGambit.BattleState.partyCharacters)
                {
                    count++;
                    if (pc2 == pc)
                        return count;
                }
            }

            return 0;
        }

        static public PartyCharacter GetCharacterFromCharacterIndex(int index)
        {
            int count = 0;
            if (index < 0)
            {
                index = -index;
                foreach (PartyCharacter pc2 in FinalGambit.BattleState.foeCharacters)
                {
                    count++;
                    if (count == index)
                        return pc2;
                }
            }
            else if (index > 0)
            {
                foreach (PartyCharacter pc2 in FinalGambit.BattleState.partyCharacters)
                {
                    count++;
                    if (count == index)
                        return pc2;
                }
            }

            return null;
        }

        static public bool AreBattleActionAndTargetLegal(int battleActionID, PartyCharacter target, bool writeErrorToConsole)
        {

            bool isLegal = true;

            LegalActionAndRequirments actionAndLegalReqs = null;

            foreach (LegalActionAndRequirments legalAction in FinalGambit.BattleState.legalActions)
            {
                if (legalAction.actionID == battleActionID)
                {
                    actionAndLegalReqs = legalAction;
                    break;
                }
            }

            if (actionAndLegalReqs == null)
            {
                isLegal = false;

                if (writeErrorToConsole)
                    Console.WriteLine(BattleActionID.lookUp[battleActionID] + " is not in the list of possible actions!");
            }

            if (actionAndLegalReqs != null)
            {
                if (target == null && !actionAndLegalReqs.legalities.Contains(LegalTargetIdentifiers.NoTargetRequired))
                {
                    if (writeErrorToConsole)
                        Console.WriteLine(BattleActionID.lookUp[battleActionID] + " requires a target!");
                    isLegal = false;
                }

                if (target != null)
                {
                    if (target.hp > 0)
                    {
                        if (actionAndLegalReqs.legalities.Contains(LegalTargetIdentifiers.TargetSlain))
                        {
                            if (writeErrorToConsole)
                                Console.WriteLine(BattleActionID.lookUp[battleActionID] + " requires a slain target!");
                            isLegal = false;
                        }
                    }
                    else
                    {
                        if (!actionAndLegalReqs.legalities.Contains(LegalTargetIdentifiers.TargetSlain))
                        {
                            if (writeErrorToConsole)
                                Console.WriteLine(BattleActionID.lookUp[battleActionID] + " cannot target a slain character!");
                            isLegal = false;
                        }
                    }


                    int ind = GetCharacterIndexFromCharacter(target);

                    //Console.WriteLine("ind = " + ind + "  " + actionAndLegalReqs.legalities.Contains(LegalTargetIdentifiers.TargetFoe) + "    " + actionAndLegalReqs.legalities.Contains(LegalTargetIdentifiers.TargetAlly));

                    if (ind > 0)
                    {
                        if (actionAndLegalReqs.legalities.Contains(LegalTargetIdentifiers.TargetFoe))
                        {
                            if (writeErrorToConsole)
                                Console.WriteLine(BattleActionID.lookUp[battleActionID] + " cannot target an ally, must target a foe!");
                            isLegal = false;
                        }
                    }
                    else if (ind < 0)
                    {
                        if (actionAndLegalReqs.legalities.Contains(LegalTargetIdentifiers.TargetAlly))
                        {
                            if (writeErrorToConsole)
                                Console.WriteLine(BattleActionID.lookUp[battleActionID] + " cannot target a foe, must target an ally!");
                            isLegal = false;
                        }
                    }
                }

            }

            return isLegal;
        }

        static public class CharacterClassID
        {
            public const int Fighter = 0;
            public const int Wizard = 1;
            public const int Cleric = 2;
            public const int Rogue = 3;

            public const int Monk = 4;
            public const int Alchemist = 5;

            static public Dictionary<int, string> lookUp;

            static public void Init()
            {
                lookUp = new Dictionary<int, string>();
                lookUp.Add(CharacterClassID.Fighter, "Fighter");
                lookUp.Add(CharacterClassID.Wizard, "Wizard");
                lookUp.Add(CharacterClassID.Cleric, "Cleric");
                lookUp.Add(CharacterClassID.Rogue, "Rogue");
                lookUp.Add(CharacterClassID.Monk, "Monk");
                lookUp.Add(CharacterClassID.Alchemist, "Alchemist");
            }

        }

        static public class BattleActionID
        {
            public const int NoValue = -1;
            public const int Attack = 0;
            public const int Steal = 1;

            public const int Haste = 13;
            public const int Slow = 14;

            public const int Faith = 19;
            public const int Brave = 20;
            public const int Debrave = 21;
            public const int Defaith = 22;

            public const int Petrify = 27;

            public const int AutoLife = 30;
            public const int Doom = 31;

            public const int Esuna = 33;
            public const int Chakra = 34;

            public const int Dispel = 132;

            public const int PoisonStrike = 141;
            public const int FlurryOfBlows = 144;


            public const int Potion = 200;
            public const int Ether = 202;
            public const int Elixer = 204;
            public const int MegaElixer = 205;
            public const int Revive = 206;
            public const int SilenceRemedy = 207;
            public const int PoisonRemedy = 208;
            public const int PetrifyRemedy = 210;
            public const int FullRemedy = 211;



            public const int CraftPotion = 300;
            public const int CraftEther = 302;
            public const int CraftElixer = 304;
            public const int CraftMegaElixer = 305;
            public const int CraftRevive = 306;
            public const int CraftSilenceRemedy = 307;
            public const int CraftPoisonRemedy = 308;
            public const int CraftPetrifyRemedy = 310;
            public const int CraftFullRemedy = 311;

            public const int QuickHit = 410;
            public const int StunStrike = 411;

            public const int MagicMissile = 501;
            public const int FlameStrike = 502;
            public const int Fireball = 503;
            public const int Meteor = 505;


            public const int CureLight = 551;
            public const int CureSerious = 552;

            public const int MassHeal = 554;
            public const int Resurrection = 555;


            public const int ManaBurn = 601;
            public const int PoisonNova = 602;
            public const int SilenceStrike = 603;


            static public Dictionary<int, string> lookUp;
            static List<int> offensiveIDs;
            static List<int> requireNoTargetsIDs;

            static List<int> canBeCoveredIDs;

            static public void Init()
            {
                lookUp = new Dictionary<int, string>();
                lookUp.Add(BattleActionID.NoValue, "");
                lookUp.Add(BattleActionID.Attack, "Attack");
                lookUp.Add(BattleActionID.Steal, "Steal");

                lookUp.Add(BattleActionID.Haste, "Haste");
                lookUp.Add(BattleActionID.Slow, "Slow");

                lookUp.Add(BattleActionID.Faith, "Faith");
                lookUp.Add(BattleActionID.Brave, "Brave");
                lookUp.Add(BattleActionID.Debrave, "Debrave");
                lookUp.Add(BattleActionID.Defaith, "Defaith");

                lookUp.Add(BattleActionID.Petrify, "Petrify");
                lookUp.Add(BattleActionID.AutoLife, "Auto Life");
                lookUp.Add(BattleActionID.Doom, "Doom");

                lookUp.Add(BattleActionID.Esuna, "Esuna");
                lookUp.Add(BattleActionID.Chakra, "Chakra");

                lookUp.Add(BattleActionID.MagicMissile, "Magic Missile");
                lookUp.Add(BattleActionID.FlameStrike, "Flame Strike");
                lookUp.Add(BattleActionID.Fireball, "Fireball");

                lookUp.Add(BattleActionID.PoisonNova, "Poison Nova");

                lookUp.Add(BattleActionID.Meteor, "Meteor");

                lookUp.Add(BattleActionID.ManaBurn, "Mana Burn");

                lookUp.Add(BattleActionID.Dispel, "Dispel");


                lookUp.Add(BattleActionID.CureLight, "Cure Light");
                lookUp.Add(BattleActionID.CureSerious, "Cure Serious");
                lookUp.Add(BattleActionID.MassHeal, "Mass Heal");
                lookUp.Add(BattleActionID.Resurrection, "Resurrection");


                lookUp.Add(BattleActionID.PoisonStrike, "Poison Strike");
                lookUp.Add(BattleActionID.FlurryOfBlows, "Flurry Of Blows");

                lookUp.Add(BattleActionID.QuickHit, "Quick Hit");
                lookUp.Add(BattleActionID.StunStrike, "Stun Strike");

                lookUp.Add(BattleActionID.SilenceStrike, "Silence Strike");



                lookUp.Add(BattleActionID.Potion, "Potion");
                lookUp.Add(BattleActionID.Ether, "Ether");
                lookUp.Add(BattleActionID.Elixer, "Elixer");
                lookUp.Add(BattleActionID.MegaElixer, "Mega Elixer");
                lookUp.Add(BattleActionID.Revive, "Revive");
                lookUp.Add(BattleActionID.SilenceRemedy, "Silence Remedy");
                lookUp.Add(BattleActionID.PoisonRemedy, "Poison Remedy");
                lookUp.Add(BattleActionID.PetrifyRemedy, "Petrify Remedy");
                lookUp.Add(BattleActionID.FullRemedy, "Full Remedy");


                lookUp.Add(BattleActionID.CraftPotion, "Craft Potion");
                lookUp.Add(BattleActionID.CraftEther, "Craft Ether");
                lookUp.Add(BattleActionID.CraftElixer, "Craft Elixer");
                lookUp.Add(BattleActionID.CraftMegaElixer, "Craft Mega Elixer");
                lookUp.Add(BattleActionID.CraftRevive, "Craft Revive");
                lookUp.Add(BattleActionID.CraftSilenceRemedy, "Craft Silence Remedy");
                lookUp.Add(BattleActionID.CraftPoisonRemedy, "Craft Poison Remedy");
                lookUp.Add(BattleActionID.CraftPetrifyRemedy, "Craft Petrify Remedy");
                lookUp.Add(BattleActionID.CraftFullRemedy, "Craft Full Remedy");



                offensiveIDs = new List<int>(){
            BattleActionID.Attack,
            BattleActionID.Steal,
            BattleActionID.Slow,
            BattleActionID.Debrave,
            BattleActionID.Defaith,
            BattleActionID.Petrify,
            BattleActionID.Doom,

            BattleActionID.MagicMissile,
            BattleActionID.FlameStrike,
            BattleActionID.Fireball,
            BattleActionID.Meteor,
            BattleActionID.ManaBurn,


            BattleActionID.Dispel,
            BattleActionID.PoisonStrike,

            BattleActionID.FlurryOfBlows,
            BattleActionID.QuickHit,
            BattleActionID.StunStrike,


            BattleActionID.SilenceStrike,
            BattleActionID.PoisonNova,

            };

                requireNoTargetsIDs = new List<int>(){
                Steal,

                BattleActionID.Fireball,
                BattleActionID.Meteor,

                FlurryOfBlows,

                MegaElixer,
                CraftPotion,
                CraftEther,
                CraftElixer,
                CraftMegaElixer,
                CraftRevive,
                CraftSilenceRemedy,
                CraftPoisonRemedy,
                CraftPetrifyRemedy,
                CraftFullRemedy,

                MassHeal,

                PoisonNova


        };


                canBeCoveredIDs = new List<int>() { Attack, PoisonStrike, QuickHit, StunStrike, SilenceStrike };

            }

            static public bool IsOffensiveID(int battleActionID)
            {
                return offensiveIDs.Contains(battleActionID);
            }

            static public bool IsBattleActionItem(int battleActionID)
            {
                //Debug.Log("checking " + battleActionID);
                string battleActionName = lookUp[battleActionID];
                foreach (int i in ItemID.lookUp.Keys)
                {
                    if (ItemID.lookUp[i] == battleActionName)
                        return true;
                }

                return false;
            }

            static public bool HasTargetAsRequirement(int battleActionID)
            {
                return !requireNoTargetsIDs.Contains(battleActionID);
            }

            static public bool CanBattleActionBeCovered(int battleActionID)
            {
                return canBeCoveredIDs.Contains(battleActionID);
            }

        }

        static public class ItemID
        {

            public const int Potion = 200;
            public const int Ether = 202;
            public const int Elixer = 204;
            public const int MegaElixer = 205;
            public const int Revive = 206;
            public const int SilenceRemedy = 207;
            public const int PoisonRemedy = 208;
            public const int PetrifyRemedy = 210;
            public const int FullRemedy = 211;
            public const int CraftMaterial = 212;

            static public Dictionary<int, string> lookUp;

            static public void Init()
            {
                lookUp = new Dictionary<int, string>();
                lookUp.Add(ItemID.Potion, "Potion");
                lookUp.Add(ItemID.Ether, "Ether");
                lookUp.Add(ItemID.Elixer, "Elixer");
                lookUp.Add(ItemID.MegaElixer, "Mega Elixer");
                lookUp.Add(ItemID.Revive, "Revive");
                lookUp.Add(ItemID.SilenceRemedy, "Silence Remedy");
                lookUp.Add(ItemID.PoisonRemedy, "Poison Remedy");
                lookUp.Add(ItemID.PetrifyRemedy, "Petrify Remedy");
                lookUp.Add(ItemID.FullRemedy, "Full Remedy");
                lookUp.Add(ItemID.CraftMaterial, "Craft Material");
            }

        }

        static public class PassiveAbilityID
        {
            public const int Cover = 1;
            public const int ItemJockey = 19;
            public const int SneakAttack = 20;

            public const int Larceny = 23;

            static public Dictionary<int, string> lookUp;

            static public void Init()
            {
                lookUp = new Dictionary<int, string>();
                lookUp.Add(PassiveAbilityID.Cover, "Cover");
                lookUp.Add(PassiveAbilityID.ItemJockey, "Item Jockey");
                lookUp.Add(PassiveAbilityID.SneakAttack, "Sneak Attack");
                lookUp.Add(PassiveAbilityID.Larceny, "Larceny");
            }

        }

        static public class StatusEffectID
        {
            public const int Poison = 1;

            public const int Haste = 4;
            public const int Slow = 5;

            public const int Faith = 9;
            public const int Brave = 10;
            public const int Debrave = 11;
            public const int Defaith = 12;

            public const int Silence = 15;
            public const int Petrifying = 17;
            public const int Petrified = 18;
            public const int AutoLife = 20;
            public const int Doom = 21;


            static public Dictionary<int, string> lookUp;

            static public void Init()
            {
                lookUp = new Dictionary<int, string>();
                lookUp.Add(StatusEffectID.Poison, "Poison");
                lookUp.Add(StatusEffectID.Haste, "Haste");
                lookUp.Add(StatusEffectID.Slow, "Slow");
                lookUp.Add(StatusEffectID.Faith, "Faith");
                lookUp.Add(StatusEffectID.Brave, "Brave");
                lookUp.Add(StatusEffectID.Debrave, "Debrave");
                lookUp.Add(StatusEffectID.Defaith, "Defaith");
                lookUp.Add(StatusEffectID.Silence, "Silence");
                lookUp.Add(StatusEffectID.Petrifying, "Petrifying");
                lookUp.Add(StatusEffectID.Petrified, "Petrified");
                lookUp.Add(StatusEffectID.AutoLife, "AutoLife");
            }

        }
        public static class ItemCost
        {
            public const int Potion = 10;
            public const int Ether = 15;
            public const int Elixer = 25;
            public const int MegaElixer = 50;
            public const int Revive = 15;
            public const int SilenceRemedy = 2;
            public const int PoisonRemedy = 1;
            //public const int GysahlGreens = 3;
            public const int PetrifyRemedy = 3;
            public const int FullRemedy = 5;
            public const int CraftMaterial = 1;

            static public Dictionary<int, int> lookUpCostValueFromID;
            static public void Init()
            {
                lookUpCostValueFromID = new Dictionary<int, int>();

                lookUpCostValueFromID.Add(ItemID.Potion, Potion);
                //lookUpCostValueFromID.Add(ItemID.HighPotion, HighPotion);
                lookUpCostValueFromID.Add(ItemID.Ether, Ether);
                //lookUpCostValueFromID.Add(ItemID.HighEther, HighEther);
                lookUpCostValueFromID.Add(ItemID.Elixer, Elixer);
                lookUpCostValueFromID.Add(ItemID.MegaElixer, MegaElixer);
                lookUpCostValueFromID.Add(ItemID.Revive, Revive);
                lookUpCostValueFromID.Add(ItemID.SilenceRemedy, SilenceRemedy);
                lookUpCostValueFromID.Add(ItemID.PoisonRemedy, PoisonRemedy);
                //lookUpCostValueFromID.Add(ItemID.GysahlGreens, GysahlGreens);
                lookUpCostValueFromID.Add(ItemID.PetrifyRemedy, PetrifyRemedy);
                lookUpCostValueFromID.Add(ItemID.FullRemedy, FullRemedy);
                lookUpCostValueFromID.Add(ItemID.CraftMaterial, CraftMaterial);

            }

        }
        public static class BattleActionMPCost
        {

            static Dictionary<int, int> lookup;

            public static void Init()
            {
                lookup = new Dictionary<int, int>();

                lookup.Add(BattleActionID.Slow, 10);
                lookup.Add(BattleActionID.MagicMissile, 10);
                lookup.Add(BattleActionID.PoisonNova, 10);
                lookup.Add(BattleActionID.Petrify, 15);
                lookup.Add(BattleActionID.FlameStrike, 30);
                lookup.Add(BattleActionID.Fireball, 25);
                lookup.Add(BattleActionID.ManaBurn, 20);
                lookup.Add(BattleActionID.Meteor, 65);


                lookup.Add(BattleActionID.CureLight, 6);
                lookup.Add(BattleActionID.CureSerious, 20);
                lookup.Add(BattleActionID.MassHeal, 20);
                lookup.Add(BattleActionID.Resurrection, 25);
                lookup.Add(BattleActionID.Haste, 10);
                lookup.Add(BattleActionID.Faith, 10);
                lookup.Add(BattleActionID.Brave, 10);
                lookup.Add(BattleActionID.Defaith, 5);
                lookup.Add(BattleActionID.Debrave, 5);
                lookup.Add(BattleActionID.AutoLife, 25);
                lookup.Add(BattleActionID.Doom, 15);
                lookup.Add(BattleActionID.Esuna, 10);
                lookup.Add(BattleActionID.Dispel, 15);
                lookup.Add(BattleActionID.PoisonStrike, 3);
                lookup.Add(BattleActionID.FlurryOfBlows, 5);
                lookup.Add(BattleActionID.QuickHit, 5);
                lookup.Add(BattleActionID.StunStrike, 5);

                lookup.Add(BattleActionID.SilenceStrike, 3);
                lookup.Add(BattleActionID.Chakra, 3);
                lookup.Add(BattleActionID.CraftPotion, 10);
                lookup.Add(BattleActionID.CraftEther, 10);
                lookup.Add(BattleActionID.CraftElixer, 10);
                lookup.Add(BattleActionID.CraftMegaElixer, 10);
                lookup.Add(BattleActionID.CraftRevive, 10);
                lookup.Add(BattleActionID.CraftFullRemedy, 10);
                lookup.Add(BattleActionID.CraftSilenceRemedy, 10);
                lookup.Add(BattleActionID.CraftPoisonRemedy, 10);
                lookup.Add(BattleActionID.CraftPetrifyRemedy, 10);

            }

            public static int GetMPCostForBattleAction(int id)
            {
                if (lookup.ContainsKey(id))
                    return lookup[id];
                return 0;
            }
        }
        public static class CraftMaterialCost
        {
            static Dictionary<int, int> lookup;

            public static void Init()
            {
                lookup = new Dictionary<int, int>();

                lookup.Add(BattleActionID.CraftPotion, 1);
                lookup.Add(BattleActionID.CraftEther, 1);
                lookup.Add(BattleActionID.CraftElixer, 2);
                lookup.Add(BattleActionID.CraftMegaElixer, 3);
                lookup.Add(BattleActionID.CraftRevive, 1);
                lookup.Add(BattleActionID.CraftSilenceRemedy, 1);
                lookup.Add(BattleActionID.CraftPoisonRemedy, 1);
                lookup.Add(BattleActionID.CraftPetrifyRemedy, 1);
                lookup.Add(BattleActionID.CraftFullRemedy, 1);
            }

            public static int GetMaterialCostForBattleAction(int id)
            {
                if (lookup.ContainsKey(id))
                    return lookup[id];
                return 0;
            }

        }


        static public int CalculateDamage(int id, PartyCharacter attacker, PartyCharacter target)
        {
            int attack = 0;
            float attackModifier = 0;
            int defense = 0;
            bool isPhysical = false;

            if (id == BattleActionID.Attack || id == BattleActionID.FlurryOfBlows || id == BattleActionID.PoisonStrike || id == BattleActionID.QuickHit || id == BattleActionID.SilenceStrike || id == BattleActionID.StunStrike)
                isPhysical = true;

            if (isPhysical)
            {
                attack = attacker.strength;
                defense = target.defense;
            }
            else
            {
                attack = attacker.wisdom;
                defense = target.magicDefense;
            }

            if (id == BattleActionID.Attack)
                attackModifier = BattleActionParams.AttackMod;
            else if (id == BattleActionID.Fireball)
                attackModifier = BattleActionParams.FireballMod;
            else if (id == BattleActionID.FlameStrike)
                attackModifier = BattleActionParams.FlameStrikeMod;
            else if (id == BattleActionID.FlurryOfBlows)
                attackModifier = BattleActionParams.FlurryOfBlowsMod;
            else if (id == BattleActionID.MagicMissile)
                attackModifier = BattleActionParams.MagicMissileMod;
            else if (id == BattleActionID.Meteor)
                attackModifier = BattleActionParams.MeteorMod;
            else if (id == BattleActionID.PoisonStrike)
                attackModifier = BattleActionParams.PoisonStrikeMod;
            else if (id == BattleActionID.QuickHit)
                attackModifier = BattleActionParams.QucikHitDamageMod;
            else if (id == BattleActionID.SilenceStrike)
                attackModifier = BattleActionParams.SilenceStrikeDamageMod;
            else if (id == BattleActionID.StunStrike)
                attackModifier = BattleActionParams.StunStrikeDamageMod;
            else
                Console.WriteLine("Attack ID not found in CalculateDamage(), damage modifier is set to 0.");

            float baseDamageAmount = (float)attack * attackModifier;
            float reductionAmount = 1f - (float)defense / 1000f;
            int damageAmount = (int)(baseDamageAmount * reductionAmount);

            if (damageAmount < 0)
                damageAmount = 0;

            return damageAmount;
        }

        static public int CalculateHeal(int id, PartyCharacter healer)
        {
            int wisdom = healer.wisdom;
            float modifier = 0;
            bool isItem = false;


            if (id == BattleActionID.CureLight)
                modifier = BattleActionParams.CureLightMod;
            else if (id == BattleActionID.CureSerious)
                modifier = BattleActionParams.CureSeriousMod;
            else if (id == BattleActionID.MassHeal)
                modifier = BattleActionParams.MassHealMod;
            else if (id == BattleActionID.Resurrection)
                modifier = BattleActionParams.ResurrectionMod;

            else if (id == BattleActionID.Potion)
            {
                modifier = BattleActionParams.PotionHealAmount;
                isItem = true;
            }
            else if (id == BattleActionID.Elixer)
            {
                modifier = BattleActionParams.ElixerHealAmount;
                isItem = true;
            }
            else if (id == BattleActionID.MegaElixer)
            {
                modifier = BattleActionParams.MegaElixerHealAmount;
                isItem = true;
            }
            else if (id == BattleActionID.Revive)
            {
                modifier = BattleActionParams.PotionHealAmount;
                isItem = true;
            }
            else
                Console.WriteLine("Attack ID not found in CalculateHeal(), heal modifier is set to 0.");

            int amount;

            if (!isItem)
                amount = (int)(((float)wisdom) * modifier);
            else
                amount = (int)modifier;

            if (amount < 0)
                amount = 0;

            return amount;
        }

        static public int CalculateManaRestore(int id, PartyCharacter healer)
        {

            int wisdom = healer.wisdom;
            float modifier = 0;
            bool isItem = false;


            if (id == BattleActionID.Chakra)
                modifier = BattleActionParams.ChakraMod;
            else if (id == BattleActionID.Ether)
            {
                modifier = BattleActionParams.EtherMPHealAmount;
                isItem = true;
            }
            else if (id == BattleActionID.Elixer)
            {
                modifier = BattleActionParams.ElixerMPHealAmount;
                isItem = true;
            }
            else if (id == BattleActionID.MegaElixer)
            {
                modifier = BattleActionParams.MegaElixerMPHealAmount;
                isItem = true;
            }
            else
                Console.WriteLine("Action ID not found in CalculateManaRestore(), mana restore modifier is set to 0.");

            int amount;

            if (!isItem)
                amount = (int)(((float)wisdom) * modifier);
            else
                amount = (int)modifier;

            if (amount < 0)
                amount = 0;

            return amount;
        }

        static public int CalculateManaDamage(int id, PartyCharacter attacker, PartyCharacter target)
        {
            int attack = 0;
            float attackModifier = 0;
            int defense = 0;
            bool isPhysical = false;

            if (id == BattleActionID.Attack || id == BattleActionID.FlurryOfBlows || id == BattleActionID.PoisonStrike || id == BattleActionID.QuickHit || id == BattleActionID.SilenceStrike || id == BattleActionID.StunStrike)
                isPhysical = true;

            if (isPhysical)
            {
                attack = attacker.strength;
                defense = target.defense;
            }
            else
            {
                attack = attacker.wisdom;
                defense = target.magicDefense;
            }

            if (id == BattleActionID.ManaBurn)
                attackModifier = BattleActionParams.ManaBurnMpDamageMod;
            else
                Console.WriteLine("Attack ID not found in CalculateManaDamage(), mana damage modifier is set to 0.");

            float baseDamageAmount = (float)attack * attackModifier;
            float reductionAmount = 1f - (float)defense / 1000f;
            int damageAmount = (int)(baseDamageAmount * reductionAmount);

            if (damageAmount < 0)
                damageAmount = 0;

            return damageAmount;
        }

        static private class BattleActionParams
        {

            public const float AttackMod = 10f;

            public const float MagicMissileMod = 10f;

            public const float FlameStrikeMod = 15f;
            public const float FireballMod = 9f;

            public const float DisintegrateMod = 20f;
            public const float MeteorMod = 12f;


            public const float CureLightMod = 12.5f;
            public const float CureSeriousMod = 17.5f;
            public const float MassHealMod = 7.5f;
            public const float ResurrectionMod = 15f;


            public const float PoisonStrikeMod = 8f;
            public const float DispelStrikeMod = 8f;
            public const float ManaBurnStrikeMod = 8f;
            public const float ManaBurnStrikeMpDamageMod = 2f;


            public const float FlurryOfBlowsMod = 8f;

            public const float ChakraMod = 2f;
            public const int ChakraMaxMPReduction = -3;



            public const int PotionHealAmount = 600;
            public const int EtherMPHealAmount = 200;
            public const int ElixerHealAmount = 800;
            public const int ElixerMPHealAmount = 200;
            public const int MegaElixerHealAmount = 800;
            public const int MegaElixerMPHealAmount = 200;
            //public const int MegaElixer = 205;
            //public const int PhoenixDown = 206;
            public const int ReviveHealAmount = 500;



            public const int PotionCraftAmount = 2;
            public const int EtherCraftAmount = 1;
            public const int ElixerCraftAmount = 1;
            public const int MegaElixerCraftAmount = 1;
            public const int CraftAmountRevive = 2;
            public const int CraftAmountSelenceRemedy = 3;
            public const int CraftAmountPoisonRemedy = 3;

            public const int CraftAmountPetrifyRemedy = 3;
            public const int CraftAmountFullRemedy = 1;


            public const float QucikHitInitReplenish = 0.5f;
            public const float QucikHitDamageMod = 10f;



            public const int StunStrikeInitReduction = -50;
            public const float StunStrikeDamageMod = 10f;

            public const float ManaBurnMpDamageMod = 3f;


            public const float SilenceStrikeDamageMod = 4f;

            public const float EchoHerbsATBReplenishAmount = 0.5f;

            public const float ItemCraftingATBReplenishAmount = 0.5f;


        }




    }

    public static class FileNames
    {

        public static string action;
        public static string flag;
        public static string battleReport;
        public static string error;
        public static string battleState;

        public static void Init()
        {
            Console.WriteLine("Exchange Folder Path: " + FileExchangePath.exchangePath);

            //string[] paths = { FileExchangePath.exchangePath, "Action.txt"};
            //action = Path.Combine(paths);

            //paths = new string[] { FileExchangePath.exchangePath, "BattleReport.txt" };
            //battleReport = Path.Combine(paths);

            //paths = new string[] { FileExchangePath.exchangePath, "Flag.txt" };
            //flag = Path.Combine(paths);

            //paths = new string[] { FileExchangePath.exchangePath, "Error.txt" };
            //error = Path.Combine(paths);

            //paths = new string[] { FileExchangePath.exchangePath, "BattleState.txt" };
            //battleState = Path.Combine(paths);

            action = FileExchangePath.exchangePath + Path.DirectorySeparatorChar + "Action.txt";

            battleReport = FileExchangePath.exchangePath + Path.DirectorySeparatorChar + "BattleReport.txt";
            flag = FileExchangePath.exchangePath + Path.DirectorySeparatorChar + "Flag.txt";
            error = FileExchangePath.exchangePath + Path.DirectorySeparatorChar + "Error.txt";
            battleState = FileExchangePath.exchangePath + Path.DirectorySeparatorChar + "BattleState.txt";


            Console.WriteLine("looking for battle state in: " + battleState);


        }

    }

}

