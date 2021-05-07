using System;
using System.IO;
using System.Collections.Generic;
//using Microsoft.Office.Interop.Excel;



namespace Final_Gambit_Player_Code
{

    static public class PartyAI
    {

        static public void ProcessAI()
        {

            Console.WriteLine("Processing AI!");


            #region SampleCode


            if (FinalGambit.BattleState.characterWithInitiative.classID == FinalGambit.CharacterClassID.Fighter)
            {
                //The character with initiative is a figher, do something here...

                Console.WriteLine("this is a fighter");

            }
            else if (FinalGambit.BattleState.characterWithInitiative.classID == FinalGambit.CharacterClassID.Cleric)
            {
                //The character with initiative is a figher, do something here...

                Console.WriteLine("this is a cleric");

            }
            else if (FinalGambit.BattleState.characterWithInitiative.classID == FinalGambit.CharacterClassID.Wizard)
            {
                //The character with initiative is a figher, do something here...

                Console.WriteLine("this is a wizard");

            }



            //Find the foe with the lowest HP
            PartyCharacter target = null;

            foreach (PartyCharacter pc in FinalGambit.BattleState.foeCharacters)
            {
                if (pc.hp > 0)
                {
                    if (target == null)
                        target = pc;
                    else if (pc.hp < target.hp)
                        target = pc;
                }
            }

            //This is the line of code that tells FG that we want to perform the attack action and target the foe with the lowest HP
            FinalGambit.PerformBattleAction(FinalGambit.BattleActionID.Attack, target);




            //Console.WriteLine("Char with init: " + FinalGambit.characterWithInitiative);


            foreach (PartyCharacter pc in FinalGambit.BattleState.partyCharacters)
            {
                foreach (StatusEffect se in pc.statusEffects)
                {
                    if (se.id == FinalGambit.StatusEffectID.Poison)
                    {
                        //We have found a character that is poisoned, do something here...
                    }
                }
            }



            #endregion

        }


    }

}




