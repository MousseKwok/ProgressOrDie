﻿/*
 * Author(s): Isaiah Mann
 * Description: [to be added]
 * Usage: [no notes]
 */

using UnityEngine;

public class PlayerCharacterBehaviour : PlayerAgent 
{	
	MonoActionf onAgilityChange;
	MonoActionInt onHPChange;
	MonoActionInt onUnallocatedStatPointsChange;

	PlayerCharacter character;

	public override AgentType GetAgentType()
	{
		return AgentType.Player;
	}

	public override Unit GetUnit() {
		return GetCharacter();
	}

	public PlayerCharacter GetCharacter () {
		return character;
	}

	public void SetCharacter (PlayerCharacter character) {
		this.character = character;
		this.SetUnit(character);
		ReplenishAtTurnStart(AgentType.Player);
	}

	public void SubscribeToEarnStatPoints (MonoActionInt action){
		onUnallocatedStatPointsChange += action;
	}

	public void UnsubscribeFromEarnStatPoints (MonoActionInt action) {
		onUnallocatedStatPointsChange -= action;
	}

	public void SubscribeToHPChange (MonoActionInt action) {
		onHPChange += action;
	}

	public void UnsubscribeFromHPChange (MonoActionInt action) {
		onHPChange -= action;
	}

	public void SubscribeToAgilityChange (MonoActionf action) {
		onAgilityChange += action;
	}

	public void UnsubscribeFromAgilityChange (MonoActionf action) {
		onAgilityChange -= action;
	}

	public override void Attack () {
		base.Attack ();
		EventModule.Event(PODEvent.PlayerAttacked);
	}

	public override bool MoveToPos (Vector3 pos)
	{
		// If animated
		if(base.MoveToPos (pos)) {
			QueryAnimator(AnimParam.Bool, IS_MOVING, true);
			return true;
		} else {
			return false;
		}
	}

	protected override bool move (int deltaX, int deltaY)
	{
		if(base.move (deltaX, deltaY)) {
			EventModule.Event("PlayerWalking");
			return true;
		} else {
			return false;
		}
	}

	protected override void stopMoving ()
	{
		base.stopMoving ();
		QueryAnimator(AnimParam.Bool, IS_MOVING, false);
		EventModule.Event("StopPlayerWalking");
	}

	void Update () {
		if (isNorthKeyDown()) {
			MoveY(1);
		} else if (isSouthKeyDown()) {
			MoveY(-1);
		}
		if (isWestKeyDown()) {
			MoveX(-1);
		} else if (isEastKeyDown()) {
			MoveX(1);
		}
	}
	
	public override void UpdateRemainingHealth (int healthRemaing) {
		base.UpdateRemainingHealth (healthRemaing);
		callOnHPChange(healthRemaing);
	}

	void callOnHPChange(int healthRemaining) {
		if(onHPChange != null) {
			onHPChange(healthRemaining);
		}
	}

	public void UpdateStatPoints(int statPoints) {
		callUpdateStatPoints(statPoints);
	}

	public override bool MoveX (int dir)
	{
		if(dir > 0) {
			QueryAnimator(AnimParam.Trigger, RIGHT);
		} else if (dir < 0) {
			QueryAnimator(AnimParam.Trigger, LEFT);
		}
		return base.MoveX (dir);
	}

	public override bool MoveY (int dir)
	{
		if(dir > 0) {
			QueryAnimator(AnimParam.Trigger, BACK);
		} else if (dir < 0) {
			QueryAnimator(AnimParam.Trigger, FRONT);
		}
		return base.MoveY (dir);
	}

	public override bool ReplenishAtTurnStart(AgentType type)
	{
		if (base.ReplenishAtTurnStart(type)) {
			callAgilityChange(remainingAgilityForTurn);
			EventModule.Event(PODEvent.PlayerTurnStart);
			return true;
		} else {
			return false;
		}
	}
		
	protected override bool trySpendAgility(int agilityPointsReq)
	{
		if (base.trySpendAgility(agilityPointsReq)) {
			callAgilityChange(remainingAgilityForTurn);
			return true;
		} else {
			return false;
		}
	}

	void callAgilityChange (float agility) {
		if (onAgilityChange != null) {
			onAgilityChange(agility);
		}
	}

	void callUpdateStatPoints(int statPoints) {
		if(onUnallocatedStatPointsChange != null) {
			onUnallocatedStatPointsChange(statPoints);
		}
	}

	protected override void SubscribeEvents ()
	{
		base.SubscribeEvents ();
		EventModule.Subscribe(handlePODGameEvent);
	}

	protected override void UnsubscribeEvents ()
	{
		base.UnsubscribeEvents ();
		EventModule.Unsubscribe(handlePODGameEvent);
	}

	protected bool playerAttackEvent(PODEvent gameEvent) {
		return gameEvent == PODEvent.PlayerMeleeAttack || gameEvent == PODEvent.PlayerMagicAttack;
	}

	protected AttackType getAttackType(PODEvent combatEvent) {
		switch(combatEvent) {
			case PODEvent.PlayerMagicAttack:
				return AttackType.Magic;
			case PODEvent.PlayerMeleeAttack:
				return AttackType.Melee;
			default:
				throw new UnityException("Attack type not found");
		}
	}

	void handlePODGameEvent(PODEvent gameEvent) {
		if(playerAttackEvent(gameEvent)) {
			QueryAnimator(AnimParam.Trigger, getAttackKey(getAttackType(gameEvent)));
		} else if (gameEvent == PODEvent.StatPanelClosed) {
			ReplenishAtTurnStart(AgentType.Player);

		}
	}

}
