//using System;
//using System.IO;
//using System.Collections.Generic;
////using Microsoft.Office.Interop.Excel;



//namespace Final_Gambit_Player_Code
//{

//    static public class PartyAI
//    {

//        const float hpLowerThan10PercentMod = 1.2f;
//        const float hpLowerThan30PercentMod = 0.8f;
//        const float hpLowerThan50PercentMod = 0.4f;
//        const float hpLowerThan70PercentMod = 0.2f;

//        const float mpLowerThan10PercentMod = 1.2f;
//        const float mpLowerThan30PercentMod = 0.8f;
//        const float mpLowerThan50PercentMod = 0.4f;
//        const float mpLowerThan70PercentMod = 0.2f;


//        static public void ProcessAI()
//        {

//            ItemCost.Init();

//            float mpDamageOrHealMod = 7.5f;
//            float willKOWeightMod = 400f;
//            float willResWeightMod = 150f;

//            float itemCostThresholdMod = 10f;

//            float gilToWeightMod = 20f;
//            float gilToReduceForEachLowTierItem = 25f;




//            #region CREATE_LIST_OF_ALL_POSSIBLE_DECISIONS
//            LinkedList<PossibleDecision> possibleDecisions = new LinkedList<PossibleDecision>();

//            foreach (int id in FinalGambit.BattleActionID.lookUp.Keys)
//            {

//                //if (id == FinalGambit.BattleActionID.PhoenixDown)
//                //{
//                //    Console.WriteLine("-----checking phoenix----");
//                //    if (FinalGambit.AreBattleActionAndTargetLegal(id, null, true))
//                //        Console.WriteLine("returned true");
//                //    else
//                //        Console.WriteLine("returned true");
//                //}

//                if (FinalGambit.AreBattleActionAndTargetLegal(id, null, false))
//                    possibleDecisions.AddLast(new PossibleDecision(id, null));
//                else
//                {
//                    foreach (PartyCharacter pc in FinalGambit.BattleState.foeCharacters)
//                    {
//                        if (FinalGambit.AreBattleActionAndTargetLegal(id, pc, false))
//                            possibleDecisions.AddLast(new PossibleDecision(id, pc));
//                    }

//                    foreach (PartyCharacter pc in FinalGambit.BattleState.partyCharacters)
//                    {
//                        if (FinalGambit.AreBattleActionAndTargetLegal(id, pc, false))
//                            possibleDecisions.AddLast(new PossibleDecision(id, pc));
//                    }
//                }
//            }

//            //foreach (PossibleDecision pd in possibleDecisions)
//            //{
//            //    if(pd.battleActionID == FinalGambit.BattleActionID.PhoenixDown)
//            //    {
//            //        if (pd.target == null)
//            //        {
//            //            Console.WriteLine("Phoneix Target is null!!!");
//            //            foreach (PartyCharacter pc in FinalGambit.BattleState.partyCharacters)
//            //            {
//            //                if(pc == null)
//            //                    Console.WriteLine("null pc found");
//            //            }
//            //            foreach (PartyCharacter pc in FinalGambit.BattleState.foeCharacters)
//            //            {
//            //                if (pc == null)
//            //                    Console.WriteLine("null foe found");
//            //            }

//            //        }
//            //        else
//            //            Console.WriteLine("Target is not null!!!");
//            //    }
//            //}

//            Console.WriteLine("Legal Decision Count = " + possibleDecisions.Count);
//            #endregion

//            #region CREATE_PARADIGM_CONSIDERATIONS

//            LinkedList<ParadigmConsideration> paradigmConsiderations = new LinkedList<ParadigmConsideration>();

//            paradigmConsiderations.AddLast(CreateParadigmConsideration(ParadigmConsiderationID.FoeHasRush));
//            paradigmConsiderations.AddLast(CreateParadigmConsideration(ParadigmConsiderationID.FoeHasPetrifyLockdown));
//            paradigmConsiderations.AddLast(CreateParadigmConsideration(ParadigmConsiderationID.FoeHasBuffersAndDebuffers));
//            paradigmConsiderations.AddLast(CreateParadigmConsideration(ParadigmConsiderationID.FoeHasLateGameSustain));
//            paradigmConsiderations.AddLast(CreateParadigmConsideration(ParadigmConsiderationID.FoeHasTempo));
//            paradigmConsiderations.AddLast(CreateParadigmConsideration(ParadigmConsiderationID.PartyHasRush));
//            paradigmConsiderations.AddLast(CreateParadigmConsideration(ParadigmConsiderationID.PartyHasPetrifyLockdown));
//            paradigmConsiderations.AddLast(CreateParadigmConsideration(ParadigmConsiderationID.PartyHasBuffersAndDebuffers));
//            paradigmConsiderations.AddLast(CreateParadigmConsideration(ParadigmConsiderationID.PartyHasLateGameSustain));
//            paradigmConsiderations.AddLast(CreateParadigmConsideration(ParadigmConsiderationID.PartyHasTempo));

//            #endregion

//            #region SUBJECT_POSSIBLE_DECISSIONS_TO_CONSIDERATIONS

//            #region RemovePossibleDecisionsThatWillBeCovered

//            LinkedList<PossibleDecision> removeMes = new LinkedList<PossibleDecision>();

//            if (!FinalGambit.BattleState.characterWithInitiative.passiveAbilities.Contains(FinalGambit.PassiveAbilityID.SneakAttack))
//            {

//                LinkedList<PartyCharacter> foesWithCover = new LinkedList<PartyCharacter>();

//                foreach (PartyCharacter pc in FinalGambit.BattleState.foeCharacters)
//                {
//                    if (pc.passiveAbilities.Contains(FinalGambit.PassiveAbilityID.Cover))
//                        foesWithCover.AddLast(pc);
//                }

//                if (foesWithCover.Count > 0)
//                {
//                    foreach (PossibleDecision pd in possibleDecisions)
//                    {
//                        if (pd.target != null)
//                        {
//                            if (pd.target.classID == FinalGambit.CharacterClassID.Wizard || pd.target.classID == FinalGambit.CharacterClassID.Cleric)
//                            {
//                                if (pd.battleActionID == FinalGambit.BattleActionID.Attack || pd.battleActionID == FinalGambit.BattleActionID.PoisonStrike
//                                    || pd.battleActionID == FinalGambit.BattleActionID.DispelStrike
//                                    || pd.battleActionID == FinalGambit.BattleActionID.ManaBurnStrike
//                                    || pd.battleActionID == FinalGambit.BattleActionID.StunStrike
//                                    || pd.battleActionID == FinalGambit.BattleActionID.QuickHit)
//                                {
//                                    removeMes.AddLast(pd);
//                                }
//                            }
//                        }
//                    }
//                }

//                foreach (PossibleDecision pd in removeMes)
//                    possibleDecisions.Remove(pd);
//            }

//            #endregion

//            #region SetInitialWeights

//            foreach (PossibleDecision pd in possibleDecisions)
//            {
//                if (pd.battleActionID == FinalGambit.BattleActionID.Attack)
//                {
//                    float amount = (float)CalculateDamage(FinalGambit.BattleState.characterWithInitiative.attack, BattleActionParams.AttackMod, pd.target.defense);
//                    if (pd.target.hp <= amount)
//                        pd.weight = pd.target.hp + willKOWeightMod;
//                    else
//                        pd.weight = amount;

//                    pd.weightMultiplier += CalculateWeightModForHPPercent(pd.target);
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.MagicMissile)
//                {
//                    float amount = (float)CalculateDamage(FinalGambit.BattleState.characterWithInitiative.magicAttack, BattleActionParams.MagicMissileMod, pd.target.magicDefense);
//                    if (pd.target.hp <= amount)
//                        pd.weight = pd.target.hp + willKOWeightMod;
//                    else
//                        pd.weight = amount;

//                    pd.weightMultiplier += CalculateWeightModForHPPercent(pd.target);
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.FlameStrike)
//                {
//                    float amount = (float)CalculateDamage(FinalGambit.BattleState.characterWithInitiative.magicAttack, BattleActionParams.FlameStrikeMod, pd.target.magicDefense);
//                    if (pd.target.hp <= amount)
//                        pd.weight = pd.target.hp + willKOWeightMod;
//                    else
//                        pd.weight = amount;

//                    pd.weightMultiplier += CalculateWeightModForHPPercent(pd.target);
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.Disintegrate)
//                {
//                    float amount = (float)CalculateDamage(FinalGambit.BattleState.characterWithInitiative.magicAttack, BattleActionParams.DisintegrateMod, pd.target.magicDefense);
//                    if (pd.target.hp <= amount)
//                        pd.weight = pd.target.hp + willKOWeightMod;
//                    else
//                        pd.weight = amount;

//                    pd.weightMultiplier += CalculateWeightModForHPPercent(pd.target);
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.Fireball)
//                {
//                    float avgMod = 0f;

//                    foreach (PartyCharacter pc in FinalGambit.BattleState.foeCharacters)
//                    {
//                        if (pc.hp <= 0)
//                            continue;

//                        float amount = (float)CalculateDamage(FinalGambit.BattleState.characterWithInitiative.magicAttack, BattleActionParams.FireballMod, pc.magicDefense);
//                        if (pc.hp <= amount)
//                            pd.weight += pc.hp + willKOWeightMod;
//                        else
//                            pd.weight += amount;

//                        avgMod += CalculateWeightModForHPPercent(pc);
//                    }

//                    avgMod = avgMod / (float)FinalGambit.BattleState.foeCharacters.Count;
//                    pd.weightMultiplier += avgMod;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.Meteor)
//                {
//                    float avgMod = 0f;

//                    foreach (PartyCharacter pc in FinalGambit.BattleState.foeCharacters)
//                    {
//                        if (pc.hp <= 0)
//                            continue;

//                        float amount = (float)CalculateDamage(FinalGambit.BattleState.characterWithInitiative.magicAttack, BattleActionParams.MeteorMod, pc.magicDefense);
//                        if (pc.hp <= amount)
//                            pd.weight += pc.hp + willKOWeightMod;
//                        else
//                            pd.weight += amount;

//                        avgMod += CalculateWeightModForHPPercent(pc);
//                    }

//                    avgMod = avgMod / (float)FinalGambit.BattleState.foeCharacters.Count;
//                    pd.weightMultiplier += avgMod;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.CureLightWounds)
//                {
//                    float amount = (float)CalculateHeal(FinalGambit.BattleState.characterWithInitiative.magicAttack, BattleActionParams.CureLightWoundsMod);
//                    if (pd.target.maxHP - pd.target.hp < amount)
//                        amount = pd.target.maxHP - pd.target.hp;
//                    pd.weight = amount;

//                    pd.weightMultiplier += CalculateWeightModForHPPercent(pd.target);
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.CureModerateWounds)
//                {
//                    float amount = (float)CalculateHeal(FinalGambit.BattleState.characterWithInitiative.magicAttack, BattleActionParams.CureModerateWoundsMod);
//                    if (pd.target.maxHP - pd.target.hp < amount)
//                        amount = pd.target.maxHP - pd.target.hp;
//                    pd.weight = amount;

//                    pd.weightMultiplier += CalculateWeightModForHPPercent(pd.target);
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.CureSeriousWounds)
//                {
//                    float amount = (float)CalculateHeal(FinalGambit.BattleState.characterWithInitiative.magicAttack, BattleActionParams.CureSeriousWoundsMod);
//                    if (pd.target.maxHP - pd.target.hp < amount)
//                        amount = pd.target.maxHP - pd.target.hp;
//                    pd.weight = amount;

//                    pd.weightMultiplier += CalculateWeightModForHPPercent(pd.target);
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.MassHeal)
//                {
//                    float avgMod = 0f;

//                    foreach (PartyCharacter pc in FinalGambit.BattleState.partyCharacters)
//                    {
//                        if (pc.hp <= 0)
//                            continue;

//                        float amount = (float)CalculateHeal(FinalGambit.BattleState.characterWithInitiative.magicAttack, BattleActionParams.MassHealMod);
//                        if (pc.maxHP - pc.hp < amount)
//                            amount = pc.maxHP - pc.hp;
//                        pd.weight += amount;

//                        avgMod += CalculateWeightModForHPPercent(pc);
//                    }

//                    avgMod = avgMod / (float)FinalGambit.BattleState.foeCharacters.Count;
//                    pd.weightMultiplier += avgMod;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.Resurrection)
//                {
//                    float amount = (float)CalculateHeal(FinalGambit.BattleState.characterWithInitiative.magicAttack, BattleActionParams.ResurrectionMod);
//                    if (pd.target.maxHP - pd.target.hp < amount)
//                        amount = pd.target.maxHP - pd.target.hp;
//                    pd.weight = amount + willResWeightMod;

//                    pd.weightMultiplier += CalculateWeightModForHPPercent(pd.target);

//                }

//                else if (pd.battleActionID == FinalGambit.BattleActionID.PoisonStrike)
//                {
//                    float amount = (float)CalculateDamage(FinalGambit.BattleState.characterWithInitiative.attack, BattleActionParams.PoisonStrikeMod, pd.target.defense);
//                    if (pd.target.hp <= amount)
//                        pd.weight = pd.target.hp + willKOWeightMod;
//                    else
//                    {
//                        pd.weight = amount;

//                        if (!HasStatusEffectID(pd.target.statusEffects, FinalGambit.StatusEffectID.Poison))
//                            pd.weight += WieghtOfStatusEfffect.Poison;
//                    }

//                    pd.weightMultiplier += CalculateWeightModForHPPercent(pd.target);
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.DispelStrike)
//                {
//                    float amount = (float)CalculateDamage(FinalGambit.BattleState.characterWithInitiative.attack, BattleActionParams.DispelStrikeMod, pd.target.defense);
//                    if (pd.target.hp <= amount)
//                        pd.weight = pd.target.hp + willKOWeightMod;
//                    else
//                    {
//                        pd.weight = amount + GetWieghtOfPositiveStatusEfffectsOnCharacter(pd.target);
//                    }

//                    pd.weightMultiplier += CalculateWeightModForHPPercent(pd.target);
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.ManaBurnStrike)
//                {
//                    float amount = (float)CalculateDamage(FinalGambit.BattleState.characterWithInitiative.attack, BattleActionParams.ManaBurnStrikeMod, pd.target.defense);
//                    if (pd.target.hp <= amount)
//                        pd.weight = pd.target.hp + willKOWeightMod;
//                    else
//                        pd.weight = amount;

//                    amount = (float)CalculateDamage(FinalGambit.BattleState.characterWithInitiative.attack, BattleActionParams.ManaBurnStrikeMpDamageMod, pd.target.defense);

//                    if (pd.target.mp < amount)
//                        amount = pd.target.mp;

//                    pd.weight += mpDamageOrHealMod * amount;

//                    pd.weightMultiplier += CalculateWeightModForHPPercent(pd.target);
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.FlurryOfBlows)
//                {
//                    float avgMod = 0f;

//                    foreach (PartyCharacter pc in FinalGambit.BattleState.foeCharacters)
//                    {
//                        float amount = (float)CalculateDamage(FinalGambit.BattleState.characterWithInitiative.attack, BattleActionParams.FlurryOfBlowsMod, pc.defense);

//                        if (pc.hp <= amount)
//                            pd.weight = pc.hp + willKOWeightMod;
//                        else
//                            pd.weight = amount;

//                        avgMod += CalculateWeightModForHPPercent(pc);
//                    }

//                    avgMod = avgMod / (float)FinalGambit.BattleState.foeCharacters.Count;
//                    pd.weightMultiplier += avgMod;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.Chakra)
//                {
//                    float amount = (float)CalculateHeal(FinalGambit.BattleState.characterWithInitiative.magicAttack, BattleActionParams.ChakraMod);
//                    if (pd.target.maxMP - pd.target.mp < amount)
//                        amount = pd.target.maxMP - pd.target.mp;
//                    pd.weight = mpDamageOrHealMod * amount;

//                    pd.weightMultiplier += CalculateWeightModForMPPercent(pd.target);
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.QuickHit)
//                {
//                    float amount = (float)CalculateDamage(FinalGambit.BattleState.characterWithInitiative.attack, BattleActionParams.QucikHitDamageMod, pd.target.defense);
//                    if (pd.target.hp <= amount)
//                        pd.weight = pd.target.hp + willKOWeightMod;
//                    else
//                        pd.weight = amount;

//                    pd.weightMultiplier += BattleActionParams.QucikHitInitReplenish;

//                    pd.weightMultiplier += CalculateWeightModForHPPercent(pd.target);

//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.StunStrike)
//                {
//                    float amount = (float)CalculateDamage(FinalGambit.BattleState.characterWithInitiative.attack, BattleActionParams.StunStrikeDamageMod, pd.target.defense);
//                    if (pd.target.hp <= amount)
//                        pd.weight = pd.target.hp + willKOWeightMod;
//                    else
//                        pd.weight = amount;

//                    pd.weightMultiplier += ((float)Math.Abs(BattleActionParams.StunStrikeInitReduction) / 100f);

//                    pd.weightMultiplier += CalculateWeightModForHPPercent(pd.target);
//                }

//                else if (pd.battleActionID == FinalGambit.BattleActionID.Potion)
//                {
//                    float amount = BattleActionParams.PotionHealAmount;
//                    if (pd.target.maxHP - pd.target.hp < amount)
//                        amount = pd.target.maxHP - pd.target.hp;
//                    pd.weight = amount;

//                    pd.weightMultiplier += CalculateWeightModForHPPercent(pd.target);

//                    if (FinalGambit.BattleState.characterWithInitiative.passiveAbilities.Contains(FinalGambit.PassiveAbilityID.ItemJockey))
//                        pd.weightMultiplier += 0.5f;

//                    pd.threshold = itemCostThresholdMod * ItemCost.Potion;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.HighPotion)
//                {
//                    float amount = BattleActionParams.HighPotionHealAmount;
//                    if (pd.target.maxHP - pd.target.hp < amount)
//                        amount = pd.target.maxHP - pd.target.hp;
//                    pd.weight = amount;

//                    pd.weightMultiplier += CalculateWeightModForHPPercent(pd.target);

//                    if (FinalGambit.BattleState.characterWithInitiative.passiveAbilities.Contains(FinalGambit.PassiveAbilityID.ItemJockey))
//                        pd.weightMultiplier += 0.5f;

//                    pd.threshold = itemCostThresholdMod * ItemCost.HighPotion;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.Ether)
//                {
//                    float amount = BattleActionParams.EtherMPHealAmount;
//                    if (pd.target.maxMP - pd.target.mp < amount)
//                        amount = pd.target.maxMP - pd.target.mp;
//                    pd.weight = mpDamageOrHealMod * amount;

//                    pd.weightMultiplier += CalculateWeightModForMPPercent(pd.target);

//                    if (FinalGambit.BattleState.characterWithInitiative.passiveAbilities.Contains(FinalGambit.PassiveAbilityID.ItemJockey))
//                        pd.weightMultiplier += 0.5f;

//                    pd.threshold = itemCostThresholdMod * ItemCost.Ether;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.HighEther)
//                {
//                    float amount = BattleActionParams.HighEtherMPHealAmount;
//                    if (pd.target.maxMP - pd.target.mp < amount)
//                        amount = pd.target.maxMP - pd.target.mp;
//                    pd.weight = mpDamageOrHealMod * amount;

//                    pd.weightMultiplier += CalculateWeightModForMPPercent(pd.target);

//                    if (FinalGambit.BattleState.characterWithInitiative.passiveAbilities.Contains(FinalGambit.PassiveAbilityID.ItemJockey))
//                        pd.weightMultiplier += 0.5f;

//                    pd.threshold = itemCostThresholdMod * ItemCost.HighEther;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.Elixer)
//                {
//                    float amount = BattleActionParams.ElixerHealAmount;
//                    if (pd.target.maxHP - pd.target.hp < amount)
//                        amount = pd.target.maxHP - pd.target.hp;
//                    pd.weight = amount;

//                    amount = BattleActionParams.ElixerMPHealAmount;
//                    if (pd.target.maxMP - pd.target.mp < amount)
//                        amount = pd.target.maxMP - pd.target.mp;
//                    pd.weight = mpDamageOrHealMod * amount;

//                    float avgMod = 0;
//                    avgMod += CalculateWeightModForHPPercent(pd.target);
//                    avgMod += CalculateWeightModForMPPercent(pd.target);
//                    avgMod = avgMod / 2f;

//                    pd.weightMultiplier += avgMod;

//                    if (FinalGambit.BattleState.characterWithInitiative.passiveAbilities.Contains(FinalGambit.PassiveAbilityID.ItemJockey))
//                        pd.weightMultiplier += 0.5f;

//                    pd.threshold = itemCostThresholdMod * ItemCost.Elixer;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.MegaElixer)
//                {

//                    float avgMod = 0;

//                    foreach (PartyCharacter pc in FinalGambit.BattleState.partyCharacters)
//                    {
//                        if (pc.hp <= 0)
//                            continue;

//                        float amount = BattleActionParams.MegaElixerHealAmount;
//                        if (pc.maxHP - pc.hp < amount)
//                            amount = pc.maxHP - pc.hp;
//                        pd.weight += amount;

//                        amount = BattleActionParams.MegaElixerMPHealAmount;
//                        if (pc.maxMP - pc.mp < amount)
//                            amount = pc.maxMP - pc.mp;
//                        pd.weight = mpDamageOrHealMod * amount;


//                        avgMod += CalculateWeightModForHPPercent(pc);
//                        avgMod += CalculateWeightModForMPPercent(pc);
//                    }

//                    avgMod = avgMod / ((float)FinalGambit.BattleState.partyCharacters.Count * 2f);

//                    pd.weightMultiplier += avgMod;

//                    if (FinalGambit.BattleState.characterWithInitiative.passiveAbilities.Contains(FinalGambit.PassiveAbilityID.ItemJockey))
//                        pd.weightMultiplier += 0.5f;

//                    pd.threshold = itemCostThresholdMod * ItemCost.MegaElixer;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.PhoenixDown)
//                {
//                    pd.weight = BattleActionParams.PhoenixDownHealAmount + willResWeightMod;

//                    pd.weightMultiplier += CalculateWeightModForHPPercent(pd.target);

//                    if (FinalGambit.BattleState.characterWithInitiative.passiveAbilities.Contains(FinalGambit.PassiveAbilityID.ItemJockey))
//                        pd.weightMultiplier += 0.5f;

//                    pd.threshold = itemCostThresholdMod * ItemCost.PhoenixDown;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.Haste)
//                {
//                    if (!HasStatusEffectID(pd.target.statusEffects, FinalGambit.StatusEffectID.Haste))
//                        pd.weight = WieghtOfStatusEfffect.Haste;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.Slow)
//                {
//                    if (!HasStatusEffectID(pd.target.statusEffects, FinalGambit.StatusEffectID.Slow))
//                        pd.weight = WieghtOfStatusEfffect.Slow;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.Regen)
//                {
//                    if (!HasStatusEffectID(pd.target.statusEffects, FinalGambit.StatusEffectID.Regen))
//                        pd.weight = WieghtOfStatusEfffect.Regen;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.Protect)
//                {
//                    if (!HasStatusEffectID(pd.target.statusEffects, FinalGambit.StatusEffectID.Protect))
//                        pd.weight = WieghtOfStatusEfffect.Protect;

//                    if (pd.target.passiveAbilities.Contains(FinalGambit.PassiveAbilityID.Cover))
//                        pd.weightMultiplier += 0.5f;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.Shell)
//                {
//                    if (!HasStatusEffectID(pd.target.statusEffects, FinalGambit.StatusEffectID.Shell))
//                        pd.weight = WieghtOfStatusEfffect.Shell;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.Faith)
//                {
//                    if (!HasStatusEffectID(pd.target.statusEffects, FinalGambit.StatusEffectID.Faith))
//                        pd.weight = WieghtOfStatusEfffect.Faith;

//                    if (pd.target.classID == FinalGambit.CharacterClassID.Cleric || pd.target.classID == FinalGambit.CharacterClassID.Wizard)
//                        pd.weightMultiplier += 0.5f;
//                    else if (pd.target.classID == FinalGambit.CharacterClassID.Alchemist)
//                        pd.weightMultiplier += 0.25f;
//                    else if (pd.target.classID == FinalGambit.CharacterClassID.Fighter || pd.target.classID == FinalGambit.CharacterClassID.Rogue || pd.target.classID == FinalGambit.CharacterClassID.Monk)
//                        pd.weightMultiplier -= 0.25f;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.Brave)
//                {
//                    if (!HasStatusEffectID(pd.target.statusEffects, FinalGambit.StatusEffectID.Brave))
//                        pd.weight = WieghtOfStatusEfffect.Brave;

//                    if (pd.target.classID == FinalGambit.CharacterClassID.Fighter)
//                        pd.weightMultiplier += 0.5f;
//                    else if (pd.target.classID == FinalGambit.CharacterClassID.Monk)
//                        pd.weightMultiplier += 0.25f;
//                    else if (pd.target.classID == FinalGambit.CharacterClassID.Wizard || pd.target.classID == FinalGambit.CharacterClassID.Cleric)
//                        pd.weightMultiplier -= 0.25f;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.Debrave)
//                {
//                    if (!HasStatusEffectID(pd.target.statusEffects, FinalGambit.StatusEffectID.Debrave))
//                        pd.weight = WieghtOfStatusEfffect.Debrave;

//                    if (pd.target.classID == FinalGambit.CharacterClassID.Fighter)
//                        pd.weightMultiplier += 0.5f;
//                    else if (pd.target.classID == FinalGambit.CharacterClassID.Monk)
//                        pd.weightMultiplier += 0.25f;
//                    else if (pd.target.classID == FinalGambit.CharacterClassID.Wizard || pd.target.classID == FinalGambit.CharacterClassID.Cleric)
//                        pd.weightMultiplier -= 0.25f;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.Defaith)
//                {
//                    if (!HasStatusEffectID(pd.target.statusEffects, FinalGambit.StatusEffectID.Defaith))
//                        pd.weight = WieghtOfStatusEfffect.Defaith;

//                    if (pd.target.classID == FinalGambit.CharacterClassID.Cleric || pd.target.classID == FinalGambit.CharacterClassID.Wizard)
//                        pd.weightMultiplier += 0.5f;
//                    else if (pd.target.classID == FinalGambit.CharacterClassID.Alchemist)
//                        pd.weightMultiplier += 0.25f;
//                    else if (pd.target.classID == FinalGambit.CharacterClassID.Fighter || pd.target.classID == FinalGambit.CharacterClassID.Rogue || pd.target.classID == FinalGambit.CharacterClassID.Monk)
//                        pd.weightMultiplier -= 0.25f;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.Deprotect)
//                {
//                    if (!HasStatusEffectID(pd.target.statusEffects, FinalGambit.StatusEffectID.Deprotect))
//                        pd.weight = WieghtOfStatusEfffect.Deprotect;

//                    if (pd.target.passiveAbilities.Contains(FinalGambit.PassiveAbilityID.Cover))
//                        pd.weightMultiplier += 0.5f;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.Deshell)
//                {
//                    if (!HasStatusEffectID(pd.target.statusEffects, FinalGambit.StatusEffectID.Deshell))
//                        pd.weight = WieghtOfStatusEfffect.Deshell;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.Silence)
//                {
//                    if (!HasStatusEffectID(pd.target.statusEffects, FinalGambit.StatusEffectID.Silence))
//                        pd.weight = WieghtOfStatusEfffect.Silence;

//                    if (pd.target.classID == FinalGambit.CharacterClassID.Cleric || pd.target.classID == FinalGambit.CharacterClassID.Wizard)
//                        pd.weightMultiplier += 0.5f;
//                    else if (pd.target.classID == FinalGambit.CharacterClassID.Alchemist || pd.target.classID == FinalGambit.CharacterClassID.Monk)
//                        pd.weightMultiplier += 0.25f;
//                    else
//                        pd.weightMultiplier += 0.25f;

//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.Petrify)
//                {
//                    if (!HasStatusEffectID(pd.target.statusEffects, FinalGambit.StatusEffectID.Petrifying) || !HasStatusEffectID(pd.target.statusEffects, FinalGambit.StatusEffectID.Petrified))
//                        pd.weight = WieghtOfStatusEfffect.Petrifying;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.Bubble)
//                {
//                    if (!HasStatusEffectID(pd.target.statusEffects, FinalGambit.StatusEffectID.Bubble))
//                        pd.weight = WieghtOfStatusEfffect.Bubble;

//                    if (pd.target.passiveAbilities.Contains(FinalGambit.PassiveAbilityID.Cover))
//                        pd.weightMultiplier += 0.5f;

//                    if (HasStatusEffectID(pd.target.statusEffects, FinalGambit.StatusEffectID.AutoLife))
//                        pd.weightMultiplier -= 0.25f;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.AutoLife)
//                {

//                    if (!HasStatusEffectID(pd.target.statusEffects, FinalGambit.StatusEffectID.AutoLife))
//                        pd.weight = WieghtOfStatusEfffect.AutoLife;

//                    if (pd.target.passiveAbilities.Contains(FinalGambit.PassiveAbilityID.Cover))
//                        pd.weightMultiplier += 0.5f;

//                    if (pd.target.classID == FinalGambit.CharacterClassID.Cleric)
//                        pd.weightMultiplier += 0.25f;

//                    if (HasStatusEffectID(pd.target.statusEffects, FinalGambit.StatusEffectID.Bubble))
//                        pd.weightMultiplier -= 0.25f;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.Doom)
//                {
//                    if (!HasStatusEffectID(pd.target.statusEffects, FinalGambit.StatusEffectID.Doom))
//                        pd.weight = WieghtOfStatusEfffect.Doom;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.EchoHerbs)
//                {
//                    foreach (StatusEffect se in pd.target.statusEffects)
//                    {
//                        if (se.id == FinalGambit.StatusEffectID.Silence)
//                        {
//                            pd.weight = WieghtOfStatusEfffect.Silence;
//                            break;
//                        }
//                    }

//                    pd.threshold = itemCostThresholdMod * ItemCost.EchoHerbs;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.Antidote)
//                {
//                    foreach (StatusEffect se in pd.target.statusEffects)
//                    {
//                        if (se.id == FinalGambit.StatusEffectID.Poison)
//                        {
//                            pd.weight = WieghtOfStatusEfffect.Poison;
//                            break;
//                        }
//                    }

//                    pd.threshold = itemCostThresholdMod * ItemCost.Antidote;
//                }

//                else if (pd.battleActionID == FinalGambit.BattleActionID.GoldenNeedle)
//                {
//                    foreach (StatusEffect se in pd.target.statusEffects)
//                    {
//                        if (se.id == FinalGambit.StatusEffectID.Petrified)
//                        {
//                            pd.weight = WieghtOfStatusEfffect.Petrified;
//                            break;
//                        }
//                        else if (se.id == FinalGambit.StatusEffectID.Petrifying)
//                        {
//                            pd.weight = WieghtOfStatusEfffect.Petrifying;
//                            break;
//                        }
//                    }

//                    pd.threshold = itemCostThresholdMod * ItemCost.GoldenNeedle;

//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.Esuna)
//                {
//                    pd.weight = GetWieghtOfNegativeStatusEfffectsOnCharacter(pd.target);
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.Remedy)
//                {
//                    pd.weight = GetWieghtOfNegativeStatusEfffectsOnCharacter(pd.target);
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.Dispel)
//                {
//                    pd.weight = GetWieghtOfPositiveStatusEfffectsOnCharacter(pd.target);
//                }

//                else if (pd.battleActionID == FinalGambit.BattleActionID.CraftPotion)
//                {
//                    pd.weight = ItemCost.Potion * gilToWeightMod;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.CraftHighPotion)
//                {
//                    pd.weight = ItemCost.HighPotion * gilToWeightMod;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.CraftEther)
//                {
//                    pd.weight = ItemCost.Ether * gilToWeightMod;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.CraftHighEther)
//                {
//                    pd.weight = ItemCost.HighEther * gilToWeightMod;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.CraftElixer)
//                {
//                    pd.weight = ItemCost.Elixer * gilToWeightMod;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.CraftMegaElixer)
//                {
//                    pd.weight = ItemCost.MegaElixer * gilToWeightMod;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.CraftPhoenixDown)
//                {
//                    pd.weight = ItemCost.PhoenixDown * gilToWeightMod;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.CraftEchoHerbs)
//                {
//                    pd.weight = ItemCost.EchoHerbs * gilToWeightMod;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.CraftAntidote)
//                {
//                    pd.weight = ItemCost.Antidote * gilToWeightMod;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.CraftGoldenNeedle)
//                {
//                    pd.weight = ItemCost.GoldenNeedle * gilToWeightMod;
//                }
//                else if (pd.battleActionID == FinalGambit.BattleActionID.CraftRemedy)
//                {
//                    pd.weight = ItemCost.Remedy * gilToWeightMod;
//                }

//                else if (pd.battleActionID == FinalGambit.BattleActionID.Steal)
//                {
//                    float highestValue = 0;

//                    float itemCountThatCostLessThan11Gil = 0;

//                    foreach (InventoryItem ii in FinalGambit.BattleState.foeInventory)
//                    {
//                        if (ii.count < 1)
//                            continue;

//                        if (ItemCost.lookUpCostValueFromID[ii.id] < 11)
//                            itemCountThatCostLessThan11Gil++;
//                        else
//                        {
//                            if (highestValue < ItemCost.lookUpCostValueFromID[ii.id])
//                                highestValue = ItemCost.lookUpCostValueFromID[ii.id];
//                        }
//                    }

//                    pd.weight = (gilToWeightMod * highestValue) - (itemCountThatCostLessThan11Gil * gilToReduceForEachLowTierItem);

//                }

//            }

//            #endregion

//            #region FactorInMPCost

//            foreach (PossibleDecision pd in possibleDecisions)
//            {
//                float mpCost = FinalGambit.GetMPCostForBattleAction(pd.battleActionID);
//                pd.weight -= (mpCost * mpDamageOrHealMod / 2f);
//            }

//            #endregion

//            #endregion

//            #region DETERMINE_WHICH_POSSIBLE_DECISION_HAS_GREATEST_WEIGHT

//            PossibleDecision actionToPerform = null;
//            foreach (PossibleDecision pd in possibleDecisions)
//            {
//                if (pd.weight * pd.weightMultiplier < pd.threshold)
//                    continue;

//                if (actionToPerform == null)
//                {
//                    if (pd.weight > 0)
//                        actionToPerform = pd;
//                }
//                else if (pd.weight * pd.weightMultiplier > actionToPerform.weight * actionToPerform.weightMultiplier)
//                    actionToPerform = pd;
//            }


//            if (actionToPerform != null)
//            {
//                if (actionToPerform.target != null)
//                    Console.WriteLine(FinalGambit.BattleActionID.lookUp[actionToPerform.battleActionID] + " selected, targeting " + FinalGambit.CharacterClassID.lookUp[actionToPerform.target.classID]);
//                else
//                    Console.WriteLine(FinalGambit.BattleActionID.lookUp[actionToPerform.battleActionID] + " selected");

//                FinalGambit.PerformBattleAction(actionToPerform.battleActionID, actionToPerform.target);
//            }
//            else
//            {
//                Console.WriteLine("No possible action selected, defaulting to attack vs lowest HP target.");

//                //use dummy attack selection, lowest char hp

//                PartyCharacter target = null;

//                foreach (PartyCharacter pc in FinalGambit.BattleState.foeCharacters)
//                {
//                    if (pc.hp > 0)
//                    {
//                        if (target == null)
//                            target = pc;
//                        else if (pc.hp < target.hp)
//                            target = pc;
//                    }
//                }

//                FinalGambit.PerformBattleAction(FinalGambit.BattleActionID.Attack, target);

//            }

//            #endregion

//            #region SampleCode

//            //Console.WriteLine("Char with init: " + FinalGambit.characterWithInitiative);


//            //foreach (PartyCharacter pc in FinalGambit.BattleState.partyCharacters)
//            //{
//            //    foreach(StatusEffect se in pc.statusEffects)
//            //    {
//            //        if(se.id == FinalGambit.StatusEffectID.Poison)
//            //        {
//            //            //We have found a character that is poisoned, do something here...
//            //        }
//            //    }
//            //}

//            //if(FinalGambit.characterWithInitiative.classID == FinalGambit.CharacterClassID.Fighter)
//            //{
//            //    //The character with initiative is a figher, do something here...
//            //}

//            #endregion

//        }

//        static private ParadigmConsideration CreateParadigmConsideration(int id)
//        {
//            ParadigmConsideration c = new ParadigmConsideration();

//            return c;
//        }

//        static private int CalculateDamage(int attack, float attackModifier, int defense)
//        {
//            float baseDamageAmount = (float)attack * attackModifier;
//            float reductionAmount = 1f - (float)defense / 1000f;
//            int damageAmount = (int)(baseDamageAmount * reductionAmount);

//            if (damageAmount < 0)
//                damageAmount = 0;

//            return damageAmount;
//        }

//        static private int CalculateHeal(int magicAttack, float modifier)
//        {
//            int amount = (int)(((float)magicAttack) * modifier);

//            if (amount < 0)
//                amount = 0;

//            return amount;
//        }

//        static private float CalculateWeightModForHPPercent(PartyCharacter pc)
//        {

//            float p = (float)pc.maxHP / (float)pc.hp;

//            if (p < 0.1f)
//                return hpLowerThan10PercentMod;
//            else if (p < 0.3f)
//                return hpLowerThan30PercentMod;
//            else if (p < 0.5f)
//                return hpLowerThan50PercentMod;
//            else if (p < 0.7f)
//                return hpLowerThan70PercentMod;

//            return 0;

//        }

//        static private float CalculateWeightModForMPPercent(PartyCharacter pc)
//        {

//            float p = (float)pc.maxMP / (float)pc.mp;

//            if (p < 0.1f)
//                return mpLowerThan10PercentMod;
//            else if (p < 0.3f)
//                return mpLowerThan30PercentMod;
//            else if (p < 0.5f)
//                return mpLowerThan50PercentMod;
//            else if (p < 0.7f)
//                return mpLowerThan70PercentMod;

//            return 0;

//        }

//        static private float GetWieghtOfPositiveStatusEfffectsOnCharacter(PartyCharacter pc)
//        {
//            float total = 0;

//            foreach (StatusEffect se in pc.statusEffects)
//            {
//                if (se.id == FinalGambit.StatusEffectID.AutoLife)
//                    total += WieghtOfStatusEfffect.AutoLife;
//                else if (se.id == FinalGambit.StatusEffectID.Brave)
//                    total += WieghtOfStatusEfffect.Brave;
//                else if (se.id == FinalGambit.StatusEffectID.Bubble)
//                    total += WieghtOfStatusEfffect.Bubble;
//                else if (se.id == FinalGambit.StatusEffectID.Faith)
//                    total += WieghtOfStatusEfffect.Faith;
//                else if (se.id == FinalGambit.StatusEffectID.Haste)
//                    total += WieghtOfStatusEfffect.Haste;
//                else if (se.id == FinalGambit.StatusEffectID.Protect)
//                    total += WieghtOfStatusEfffect.Protect;
//                else if (se.id == FinalGambit.StatusEffectID.Regen)
//                    total += WieghtOfStatusEfffect.Regen;
//                else if (se.id == FinalGambit.StatusEffectID.Shell)
//                    total += WieghtOfStatusEfffect.Shell;
//            }

//            return total;
//        }

//        static private float GetWieghtOfNegativeStatusEfffectsOnCharacter(PartyCharacter pc)
//        {
//            float total = 0;

//            foreach (StatusEffect se in pc.statusEffects)
//            {
//                if (se.id == FinalGambit.StatusEffectID.Debrave)
//                    total += WieghtOfStatusEfffect.Debrave;
//                else if (se.id == FinalGambit.StatusEffectID.Defaith)
//                    total += WieghtOfStatusEfffect.Defaith;
//                else if (se.id == FinalGambit.StatusEffectID.Deprotect)
//                    total += WieghtOfStatusEfffect.Deprotect;
//                else if (se.id == FinalGambit.StatusEffectID.Deshell)
//                    total += WieghtOfStatusEfffect.Deshell;
//                else if (se.id == FinalGambit.StatusEffectID.Doom)
//                    total += WieghtOfStatusEfffect.Doom;
//                else if (se.id == FinalGambit.StatusEffectID.Petrified)
//                    total += WieghtOfStatusEfffect.Petrified;
//                else if (se.id == FinalGambit.StatusEffectID.Petrifying)
//                    total += WieghtOfStatusEfffect.Petrifying;
//                else if (se.id == FinalGambit.StatusEffectID.Poison)
//                    total += WieghtOfStatusEfffect.Poison;
//                else if (se.id == FinalGambit.StatusEffectID.Silence)
//                    total += WieghtOfStatusEfffect.Silence;
//                else if (se.id == FinalGambit.StatusEffectID.Slow)
//                    total += WieghtOfStatusEfffect.Slow;

//            }

//            return total;
//        }


//        static private bool HasStatusEffectID(LinkedList<StatusEffect> statusEffects, int id)
//        {
//            foreach (StatusEffect se in statusEffects)
//            {
//                if (se.id == id)
//                    return true;
//            }

//            return false;
//        }

//    }

//    public class ParadigmConsideration
//    {
//        public float weight;
//        public int id;
//    }

//    public class PossibleDecision
//    {
//        public float weight;
//        public int battleActionID;
//        public PartyCharacter target;
//        public float weightMultiplier = 1f;
//        public float threshold;

//        public PossibleDecision()
//        {

//        }

//        public PossibleDecision(int BattleActionID, PartyCharacter Target)
//        {
//            battleActionID = BattleActionID;
//            target = Target;
//        }
//    }

//    static public class ParadigmConsiderationID
//    {
//        public const int FoeHasRush = 1;
//        public const int FoeHasPetrifyLockdown = 2;
//        public const int FoeHasBuffersAndDebuffers = 3;
//        public const int FoeHasLateGameSustain = 4;
//        public const int FoeHasTempo = 5;

//        public const int PartyHasRush = 6;
//        public const int PartyHasPetrifyLockdown = 7;
//        public const int PartyHasBuffersAndDebuffers = 8;
//        public const int PartyHasLateGameSustain = 9;
//        public const int PartyHasTempo = 10;

//        public const int FoeHealthGeneralized = 11;
//        public const int PartyHealthGeneralized = 12;

//    }

//    static public class BattleActionParams
//    {

//        public const float AttackMod = 1f;

//        public const float MagicMissileMod = 1.0f;

//        public const float FlameStrikeMod = 1.5f;
//        public const float FireballMod = 0.9f;

//        public const float DisintegrateMod = 2.0f;
//        public const float MeteorMod = 1.2f;


//        public const float CureLightWoundsMod = 1.25f;
//        public const float CureModerateWoundsMod = 1.5f;
//        public const float CureSeriousWoundsMod = 1.75f;
//        public const float MassHealMod = 0.75f;
//        public const float ResurrectionMod = 1.5f;


//        public const float PoisonStrikeMod = 0.8f;
//        public const float DispelStrikeMod = 0.8f;
//        public const float ManaBurnStrikeMod = 0.8f;
//        public const float ManaBurnStrikeMpDamageMod = 0.8f;


//        public const float FlurryOfBlowsMod = 0.6f;

//        public const float ChakraMod = 0.2f;



//        public const int PotionHealAmount = 300;
//        public const int HighPotionHealAmount = 600;
//        public const int EtherMPHealAmount = 50;
//        public const int HighEtherMPHealAmount = 200;
//        public const int ElixerHealAmount = 800;
//        public const int ElixerMPHealAmount = 200;
//        public const int MegaElixerHealAmount = 800;
//        public const int MegaElixerMPHealAmount = 200;

//        public const int PhoenixDownHealAmount = 500;



//        public const int PotionCraftAmount = 3;
//        public const int HighPotionCraftAmount = 2;
//        public const int EtherCraftAmount = 2;
//        public const int HighEtherCraftAmount = 1;
//        public const int ElixerCraftAmount = 1;
//        public const int MegaElixerCraftAmount = 1;
//        public const int CraftAmountPhoenixDown = 2;
//        public const int CraftAmountEchoHerbs = 3;
//        public const int CraftAmountAntidote = 3;
//        public const int CraftAmountGysahlGreens = 3;
//        public const int CraftAmountGoldenNeedle = 3;
//        public const int CraftAmountRemedy = 1;


//        public const float QucikHitInitReplenish = 0.5f;
//        public const float QucikHitDamageMod = 1.0f;



//        public const int StunStrikeInitReduction = -50;
//        public const float StunStrikeDamageMod = 1.0f;


//    }

//    static public class WieghtOfStatusEfffect
//    {
//        public const float Poison = 200;
//        public const float Regen = 250;

//        public const float Haste = 300;
//        public const float Slow = 300;

//        public const float Protect = 300;
//        public const float Shell = 300;
//        public const float Faith = 300;
//        public const float Brave = 300;
//        public const float Debrave = 300;
//        public const float Defaith = 300;
//        public const float Deprotect = 300;
//        public const float Deshell = 300;

//        public const float Silence = 350;
//        public const float Petrifying = 300;
//        public const float Petrified = 450;
//        public const float Bubble = 400;
//        public const float AutoLife = 450;
//        public const float Doom = 400;
//    }


//    public static class ItemCost
//    {
//        public const int Potion = 3;
//        public const int HighPotion = 10;
//        public const int Ether = 10;
//        public const int HighEther = 20;
//        public const int Elixer = 30;
//        public const int MegaElixer = 50;
//        public const int PhoenixDown = 15;
//        public const int EchoHerbs = 3;
//        public const int Antidote = 3;
//        public const int GoldenNeedle = 3;
//        public const int Remedy = 10;
//        public const int CraftMaterial = 1;

//        static public Dictionary<int, int> lookUpCostValueFromID;
//        static public void Init()
//        {
//            lookUpCostValueFromID = new Dictionary<int, int>();

//            lookUpCostValueFromID.Add(FinalGambit.ItemID.Potion, Potion);
//            lookUpCostValueFromID.Add(FinalGambit.ItemID.HighPotion, HighPotion);
//            lookUpCostValueFromID.Add(FinalGambit.ItemID.Ether, Ether);
//            lookUpCostValueFromID.Add(FinalGambit.ItemID.HighEther, HighEther);
//            lookUpCostValueFromID.Add(FinalGambit.ItemID.Elixer, Elixer);
//            lookUpCostValueFromID.Add(FinalGambit.ItemID.MegaElixer, MegaElixer);
//            lookUpCostValueFromID.Add(FinalGambit.ItemID.PhoenixDown, PhoenixDown);
//            lookUpCostValueFromID.Add(FinalGambit.ItemID.EchoHerbs, EchoHerbs);
//            lookUpCostValueFromID.Add(FinalGambit.ItemID.Antidote, Antidote);
//            lookUpCostValueFromID.Add(FinalGambit.ItemID.GoldenNeedle, GoldenNeedle);
//            lookUpCostValueFromID.Add(FinalGambit.ItemID.Remedy, Remedy);
//            lookUpCostValueFromID.Add(FinalGambit.ItemID.CraftMaterial, CraftMaterial);

//        }

//    }


//}




