using System;
using System.IO;
using System.Collections.Generic;

namespace Final_Gambit_Player_Code
{

    static public class PartyAI
    {

        static public void ProcessAI()
        {

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

            FinalGambit.PerformBattleAction(FinalGambit.BattleActionID.Attack, target);

        }
    }

}


