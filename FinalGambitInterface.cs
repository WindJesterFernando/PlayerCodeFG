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

        public int hp, maxHP, mp, maxMP, defense, magicDefense, attack, magicAttack;
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
            attack = Attack;
            magicAttack = MagicAttack;

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

                if(writeErrorToConsole)
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
                lookUp.Add(CharacterClassID.Alchemist, "Alchemis");
            }

        }

        static public class BattleActionID
        {
            public const int NoValue = -1;
            public const int Attack = 0;
            public const int Steal = 1;

            public const int Haste = 13;
            public const int Slow = 14;
            public const int Regen = 15;

            public const int Protect = 17;
            public const int Shell = 18;
            public const int Faith = 19;
            public const int Brave = 20;
            public const int Debrave = 21;
            public const int Defaith = 22;
            public const int Deprotect = 23;
            public const int Deshell = 24;




            public const int Silence = 25;

            public const int Petrify = 27;

            public const int Bubble = 29;
            public const int AutoLife = 30;
            public const int Doom = 31;

            public const int Esuna = 33;
            public const int Chakra = 34;

            public const int Dispel = 132;

            public const int PoisonStrike = 141;
            public const int DispelStrike = 142;
            public const int ManaBurnStrike = 143;
            public const int FlurryOfBlows = 144;


            public const int Potion = 200;
            public const int HighPotion = 201;
            public const int Ether = 202;
            public const int HighEther = 203;
            public const int Elixer = 204;
            public const int MegaElixer = 205;
            public const int PhoenixDown = 206;
            public const int EchoHerbs = 207;
            public const int Antidote = 208;
            public const int GoldenNeedle = 210;
            public const int Remedy = 211;



            public const int CraftPotion = 300;
            public const int CraftHighPotion = 301;
            public const int CraftEther = 302;
            public const int CraftHighEther = 303;
            public const int CraftElixer = 304;
            public const int CraftMegaElixer = 305;
            public const int CraftPhoenixDown = 306;
            public const int CraftEchoHerbs = 307;
            public const int CraftAntidote = 308;
            public const int CraftGoldenNeedle = 310;
            public const int CraftRemedy = 311;

            public const int QuickHit = 410;
            public const int StunStrike = 411;

            public const int MagicMissile = 501;
            public const int FlameStrike = 502;
            public const int Fireball = 503;
            public const int Disintegrate = 504;
            public const int Meteor = 505;


            public const int CureLightWounds = 551;
            public const int CureModerateWounds = 552;
            public const int CureSeriousWounds = 553;
            public const int MassHeal = 554;
            public const int Resurrection = 555;


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

                lookUp.Add(BattleActionID.Regen, "Regen");

                lookUp.Add(BattleActionID.Protect, "Protect");
                lookUp.Add(BattleActionID.Shell, "Shell");
                lookUp.Add(BattleActionID.Faith, "Faith");
                lookUp.Add(BattleActionID.Brave, "Brave");
                lookUp.Add(BattleActionID.Debrave, "Debrave");
                lookUp.Add(BattleActionID.Defaith, "Defaith");
                lookUp.Add(BattleActionID.Deprotect, "Deprotect");
                lookUp.Add(BattleActionID.Deshell, "Deshell");


                lookUp.Add(BattleActionID.Silence, "Silence");
                lookUp.Add(BattleActionID.Petrify, "Petrify");
                lookUp.Add(BattleActionID.Bubble, "Bubble");
                lookUp.Add(BattleActionID.AutoLife, "Auto Life");
                lookUp.Add(BattleActionID.Doom, "Doom");

                lookUp.Add(BattleActionID.Esuna, "Esuna");
                lookUp.Add(BattleActionID.Chakra, "Chakra");


                lookUp.Add(BattleActionID.MagicMissile, "Magic Missile");
                lookUp.Add(BattleActionID.FlameStrike, "Flame Strike");
                lookUp.Add(BattleActionID.Fireball, "Fireball");
                lookUp.Add(BattleActionID.Disintegrate, "Disintegrate");
                lookUp.Add(BattleActionID.Meteor, "Meteor");


                lookUp.Add(BattleActionID.Dispel, "Dispel");



                lookUp.Add(BattleActionID.CureLightWounds, "Cure Light Wounds");
                lookUp.Add(BattleActionID.CureModerateWounds, "Cure Moderate Wounds");
                lookUp.Add(BattleActionID.CureSeriousWounds, "Cure Serious Wounds");
                lookUp.Add(BattleActionID.MassHeal, "Mass Heal");
                lookUp.Add(BattleActionID.Resurrection, "Resurrection");


                lookUp.Add(BattleActionID.PoisonStrike, "Poison Strike");
                lookUp.Add(BattleActionID.DispelStrike, "Dispel Strike");
                lookUp.Add(BattleActionID.ManaBurnStrike, "Mana Burn Strike");
                lookUp.Add(BattleActionID.FlurryOfBlows, "Flurry Of Blows");

                lookUp.Add(BattleActionID.QuickHit, "Quick Hit");
                lookUp.Add(BattleActionID.StunStrike, "Stun Strike");


                lookUp.Add(BattleActionID.Potion, "Potion");
                lookUp.Add(BattleActionID.HighPotion, "High Potion");
                lookUp.Add(BattleActionID.Ether, "Ether");
                lookUp.Add(BattleActionID.HighEther, "High Ether");
                lookUp.Add(BattleActionID.Elixer, "Elixer");
                lookUp.Add(BattleActionID.MegaElixer, "Mega Elixer");
                lookUp.Add(BattleActionID.PhoenixDown, "Phoenix Down");
                lookUp.Add(BattleActionID.EchoHerbs, "Echo Herbs");
                lookUp.Add(BattleActionID.Antidote, "Antidote");
                lookUp.Add(BattleActionID.GoldenNeedle, "Golden Needle");
                lookUp.Add(BattleActionID.Remedy, "Remedy");


                lookUp.Add(BattleActionID.CraftPotion, "Craft Potion");
                lookUp.Add(BattleActionID.CraftHighPotion, "Craft High Potion");
                lookUp.Add(BattleActionID.CraftEther, "Craft Ether");
                lookUp.Add(BattleActionID.CraftHighEther, "Craft High Ether");
                lookUp.Add(BattleActionID.CraftElixer, "Craft Elixer");
                lookUp.Add(BattleActionID.CraftMegaElixer, "Craft Mega Elixer");
                lookUp.Add(BattleActionID.CraftPhoenixDown, "Craft Phoenix Down");
                lookUp.Add(BattleActionID.CraftEchoHerbs, "Craft Echo Herbs");
                lookUp.Add(BattleActionID.CraftAntidote, "Craft Antidote");
                lookUp.Add(BattleActionID.CraftGoldenNeedle, "Craft Golden Needle");
                lookUp.Add(BattleActionID.CraftRemedy, "Craft Remedy");




                offensiveIDs = new List<int>(){
            BattleActionID.Attack,
            BattleActionID.Steal,
            BattleActionID.Slow,
            BattleActionID.Debrave,
            BattleActionID.Defaith,
            BattleActionID.Deprotect,
            BattleActionID.Deshell,
            BattleActionID.Silence,
            BattleActionID.Petrify,
            BattleActionID.Doom,

            BattleActionID.MagicMissile,
            BattleActionID.FlameStrike,
            BattleActionID.Fireball,
            BattleActionID.Disintegrate,
            BattleActionID.Meteor,

            BattleActionID.Dispel,
            BattleActionID.PoisonStrike,
            BattleActionID.DispelStrike,
            BattleActionID.ManaBurnStrike,
            BattleActionID.FlurryOfBlows,
            BattleActionID.QuickHit,
            BattleActionID.StunStrike,

            };

                requireNoTargetsIDs = new List<int>(){
                Steal,

                BattleActionID.Fireball,
                BattleActionID.Meteor,

                FlurryOfBlows,

                MegaElixer,
                CraftPotion,
                CraftHighPotion,
                CraftEther,
                CraftHighEther,
                CraftElixer,
                CraftMegaElixer,
                CraftPhoenixDown,
                CraftEchoHerbs,
                CraftAntidote,
                CraftGoldenNeedle,
                CraftRemedy,

                MassHeal


        };


                canBeCoveredIDs = new List<int>() { Attack, PoisonStrike, ManaBurnStrike, DispelStrike, QuickHit, StunStrike };

            }

            static public bool IsOffensiveID(int battleActionID)
            {
                return offensiveIDs.Contains(battleActionID);
            }

            static public bool IsBattleActionItem(int battleActionID)
            {
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
            public const int HighPotion = 201;
            public const int Ether = 202;
            public const int HighEther = 203;
            public const int Elixer = 204;
            public const int MegaElixer = 205;
            public const int PhoenixDown = 206;
            public const int EchoHerbs = 207;
            public const int Antidote = 208;
            //public const int GysahlGreens = 209;
            public const int GoldenNeedle = 210;
            public const int Remedy = 211;

            public const int CraftMaterial = 212;

            static public Dictionary<int, string> lookUp;

            static public void Init()
            {
                lookUp = new Dictionary<int, string>();
                lookUp.Add(ItemID.Potion, "Potion");
                lookUp.Add(ItemID.HighPotion, "High Potion");
                lookUp.Add(ItemID.Ether, "Ether");
                lookUp.Add(ItemID.HighEther, "High Ether");
                lookUp.Add(ItemID.Elixer, "Elixer");
                lookUp.Add(ItemID.MegaElixer, "Mega Elixer");
                lookUp.Add(ItemID.PhoenixDown, "Phoenix Down");
                lookUp.Add(ItemID.EchoHerbs, "Echo Herbs");
                lookUp.Add(ItemID.Antidote, "Antidote");
                //lookUp.Add(ItemID.GysahlGreens, "Gysahl Greens");
                lookUp.Add(ItemID.GoldenNeedle, "Golden Needle");
                lookUp.Add(ItemID.Remedy, "Remedy");
                lookUp.Add(ItemID.CraftMaterial, "Craft Material");

            }

        }

        static public class PassiveAbilityID
        {
            public const int Cover = 1;
            public const int ItemJockey = 19;
            public const int SneakAttack = 20;

            static public Dictionary<int, string> lookUp;

            static public void Init()
            {
                lookUp = new Dictionary<int, string>();
                lookUp.Add(PassiveAbilityID.Cover, "Cover");
                lookUp.Add(PassiveAbilityID.ItemJockey, "Item Jockey");
                lookUp.Add(PassiveAbilityID.SneakAttack, "Sneak Attack");
            }

        }


        static public class StatusEffectID
        {
            public const int Poison = 1;
            public const int Regen = 3;

            public const int Haste = 4;
            public const int Slow = 5;

            public const int Mini = 6;
            public const int Protect = 7;
            public const int Shell = 8;
            public const int Faith = 9;
            public const int Brave = 10;
            public const int Debrave = 11;
            public const int Defaith = 12;
            public const int Deprotect = 13;
            public const int Deshell = 14;

            public const int Silence = 15;
            //public const int Beserk = 16;
            public const int Petrifying = 17;
            public const int Petrified = 18;
            public const int Bubble = 19;
            public const int AutoLife = 20;
            public const int Doom = 21;


            static public Dictionary<int, string> lookUp;

            static public void Init()
            {
                lookUp = new Dictionary<int, string>();
                lookUp.Add(StatusEffectID.Poison, "Poison");
                lookUp.Add(StatusEffectID.Regen, "Regen");
                lookUp.Add(StatusEffectID.Haste, "Haste");
                lookUp.Add(StatusEffectID.Slow, "Slow");
                lookUp.Add(StatusEffectID.Mini, "Mini");
                lookUp.Add(StatusEffectID.Protect, "Protect");
                lookUp.Add(StatusEffectID.Shell, "Shell");
                lookUp.Add(StatusEffectID.Faith, "Faith");
                lookUp.Add(StatusEffectID.Brave, "Brave");
                lookUp.Add(StatusEffectID.Debrave, "Debrave");
                lookUp.Add(StatusEffectID.Defaith, "Defaith");
                lookUp.Add(StatusEffectID.Deprotect, "Deprotect");
                lookUp.Add(StatusEffectID.Deshell, "Deshell");
                lookUp.Add(StatusEffectID.Silence, "Silence");
                //lookUp.Add(StatusEffectID.Beserk, "Beserk");
                lookUp.Add(StatusEffectID.Petrifying, "Petrifying");
                lookUp.Add(StatusEffectID.Petrified, "Petrified");
                lookUp.Add(StatusEffectID.Bubble, "Bubble");
                lookUp.Add(StatusEffectID.AutoLife, "AutoLife");



            }

        }

        static public int GetMPCostForBattleAction(int battleActionID)
        {

            if (battleActionID == BattleActionID.MagicMissile || battleActionID == BattleActionID.CureLightWounds)
                return 10;
            else if (battleActionID == BattleActionID.FlameStrike || battleActionID == BattleActionID.CureModerateWounds)
                return 15;
            else if (battleActionID == BattleActionID.CureSeriousWounds)//battleActionID == BattleActionID.Firaga || 
                return 20;
            else if (battleActionID == BattleActionID.Fireball || battleActionID == BattleActionID.MassHeal)
                return 30;
            
            else if (battleActionID == BattleActionID.Resurrection)
                return 25;

            else if (battleActionID == BattleActionID.Meteor)
                return 80;

            else if (battleActionID == BattleActionID.Haste || battleActionID == BattleActionID.Slow)
                return 15;
            else if (battleActionID == BattleActionID.Regen)
                return 15;

            else if (battleActionID == BattleActionID.Protect || battleActionID == BattleActionID.Shell || battleActionID == BattleActionID.Faith || battleActionID == BattleActionID.Brave)
                return 15;

            else if (battleActionID == BattleActionID.Debrave || battleActionID == BattleActionID.Defaith || battleActionID == BattleActionID.Deprotect || battleActionID == BattleActionID.Deshell)
                return 15;

            else if (battleActionID == BattleActionID.Silence)
                return 20;

            else if (battleActionID == BattleActionID.Petrify)
                return 15;
            else if (battleActionID == BattleActionID.Bubble)
                return 25;
            else if (battleActionID == BattleActionID.AutoLife)
                return 30;
            else if (battleActionID == BattleActionID.Doom)
                return 20;

            else if (battleActionID == BattleActionID.Esuna)
                return 25;
            else if (battleActionID == BattleActionID.Dispel)
                return 25;
            else if (battleActionID == BattleActionID.PoisonStrike)
                return 10;
            else if (battleActionID == BattleActionID.DispelStrike)
                return 25;
            else if (battleActionID == BattleActionID.ManaBurnStrike)
                return 15;
            else if (battleActionID == BattleActionID.FlurryOfBlows)
                return 20;

            else if (battleActionID == BattleActionID.CraftPotion)
                return 10;
            else if (battleActionID == BattleActionID.CraftHighPotion)
                return 10;
            else if (battleActionID == BattleActionID.CraftEther)
                return 15;
            else if (battleActionID == BattleActionID.CraftHighEther)
                return 20;
            else if (battleActionID == BattleActionID.CraftElixer)
                return 30;
            else if (battleActionID == BattleActionID.CraftMegaElixer)
                return 60;
            else if (battleActionID == BattleActionID.CraftPhoenixDown)
                return 20;
            else if (battleActionID == BattleActionID.CraftRemedy)
                return 20;
            else if (battleActionID == BattleActionID.CraftEchoHerbs || battleActionID == BattleActionID.CraftAntidote || battleActionID == BattleActionID.CraftGoldenNeedle)
                return 10;

            else if (battleActionID == BattleActionID.QuickHit)
                return 10;

            else if (battleActionID == BattleActionID.StunStrike)
                return 10;

            return 0;
        }


        static private int CalculateDamage(int attack, float attackModifier, int defense)
        {
            float baseDamageAmount = (float)attack * attackModifier;
            float reductionAmount = 1f - (float)defense / 1000f;
            int damageAmount = (int)(baseDamageAmount * reductionAmount);

            if (damageAmount < 0)
                damageAmount = 0;

            return damageAmount;
        }

        static private int CalculateHeal(int magicAttack, float modifier)
        {
            int amount = (int)(((float)magicAttack) * modifier);

            if (amount < 0)
                amount = 0;

            return amount;
        }



        static public class BattleActionParams
        {

            public const float AttackMod = 1f;

            public const float MagicMissileMod = 1.0f;

            public const float FlameStrikeMod = 1.5f;
            public const float FireballMod = 0.9f;

            public const float DisintegrateMod = 2.0f;
            public const float MeteorMod = 1.2f;


            public const float CureLightWoundsMod = 1.25f;
            public const float CureModerateWoundsMod = 1.5f;
            public const float CureSeriousWoundsMod = 1.75f;
            public const float MassHealMod = 0.75f;
            public const float ResurrectionMod = 1.5f;


            public const float PoisonStrikeMod = 0.8f;
            public const float DispelStrikeMod = 0.8f;
            public const float ManaBurnStrikeMod = 0.8f;
            public const float ManaBurnStrikeMpDamageMod = 0.2f;


            public const float FlurryOfBlowsMod = 0.6f;

            public const float ChakraMod = 0.2f;



            public const int PotionHealAmount = 300;
            public const int HighPotionHealAmount = 600;
            public const int EtherMPHealAmount = 50;
            public const int HighEtherMPHealAmount = 200;
            public const int ElixerHealAmount = 800;
            public const int ElixerMPHealAmount = 200;
            public const int MegaElixerHealAmount = 800;
            public const int MegaElixerMPHealAmount = 200;

            public const int PhoenixDownHealAmount = 500;



            public const int PotionCraftAmount = 3;
            public const int HighPotionCraftAmount = 2;
            public const int EtherCraftAmount = 2;
            public const int HighEtherCraftAmount = 1;
            public const int ElixerCraftAmount = 1;
            public const int MegaElixerCraftAmount = 1;
            public const int CraftAmountPhoenixDown = 2;
            public const int CraftAmountEchoHerbs = 3;
            public const int CraftAmountAntidote = 3;
            public const int CraftAmountGysahlGreens = 3;
            public const int CraftAmountGoldenNeedle = 3;
            public const int CraftAmountRemedy = 1;


            public const float QucikHitInitReplenish = 0.5f;
            public const float QucikHitDamageMod = 1.0f;



            public const int StunStrikeInitReduction = -50;
            public const float StunStrikeDamageMod = 1.0f;


        }

        public static class ItemCost
        {
            public const int Potion = 3;
            public const int HighPotion = 10;
            public const int Ether = 10;
            public const int HighEther = 20;
            public const int Elixer = 30;
            public const int MegaElixer = 50;
            public const int PhoenixDown = 15;
            public const int EchoHerbs = 3;
            public const int Antidote = 3;
            public const int GoldenNeedle = 3;
            public const int Remedy = 10;
            public const int CraftMaterial = 1;

            static public Dictionary<int, int> lookUpCostValueFromID;
            static public void Init()
            {
                lookUpCostValueFromID = new Dictionary<int, int>();

                lookUpCostValueFromID.Add(FinalGambit.ItemID.Potion, Potion);
                lookUpCostValueFromID.Add(FinalGambit.ItemID.HighPotion, HighPotion);
                lookUpCostValueFromID.Add(FinalGambit.ItemID.Ether, Ether);
                lookUpCostValueFromID.Add(FinalGambit.ItemID.HighEther, HighEther);
                lookUpCostValueFromID.Add(FinalGambit.ItemID.Elixer, Elixer);
                lookUpCostValueFromID.Add(FinalGambit.ItemID.MegaElixer, MegaElixer);
                lookUpCostValueFromID.Add(FinalGambit.ItemID.PhoenixDown, PhoenixDown);
                lookUpCostValueFromID.Add(FinalGambit.ItemID.EchoHerbs, EchoHerbs);
                lookUpCostValueFromID.Add(FinalGambit.ItemID.Antidote, Antidote);
                lookUpCostValueFromID.Add(FinalGambit.ItemID.GoldenNeedle, GoldenNeedle);
                lookUpCostValueFromID.Add(FinalGambit.ItemID.Remedy, Remedy);
                lookUpCostValueFromID.Add(FinalGambit.ItemID.CraftMaterial, CraftMaterial);

            }

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

            string[] paths = { FileExchangePath.exchangePath, "Action.txt"};
            action = Path.Combine(paths);

            paths = new string[] { FileExchangePath.exchangePath, "BattleReport.txt" };
            battleReport = Path.Combine(paths);

            paths = new string[] { FileExchangePath.exchangePath, "Flag.txt" };
            flag = Path.Combine(paths);

            paths = new string[] { FileExchangePath.exchangePath, "Error.txt" };
            error = Path.Combine(paths);

            paths = new string[] { FileExchangePath.exchangePath, "BattleState.txt" };
            battleState = Path.Combine(paths);
        }

    }

}







