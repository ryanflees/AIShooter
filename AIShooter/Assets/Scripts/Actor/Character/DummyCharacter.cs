using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CR
{
	public class DummyCharacter : MonoBehaviour
	{
		#region -Generated Code DO NOT MODIFY-
		// Parameters
		private const string PARAM_SPEED = "Speed"; // Float
		private readonly int PARAM_SPEED_ID = Animator.StringToHash(PARAM_SPEED);
		private const string PARAM_RIGHT = "Right"; // Float
		private readonly int PARAM_RIGHT_ID = Animator.StringToHash(PARAM_RIGHT);
		private const string PARAM_FORWARD = "Forward"; // Float
		private readonly int PARAM_FORWARD_ID = Animator.StringToHash(PARAM_FORWARD);
		private const string PARAM_TRIGGERSHOWRIFLE = "TriggerShowRifle"; // Trigger
		private readonly int PARAM_TRIGGERSHOWRIFLE_ID = Animator.StringToHash(PARAM_TRIGGERSHOWRIFLE);
		private const string PARAM_TRIGGERHIDERIFLE = "TriggerHideRifle"; // Trigger
		private readonly int PARAM_TRIGGERHIDERIFLE_ID = Animator.StringToHash(PARAM_TRIGGERHIDERIFLE);
		private const string PARAM_ISCROUCH = "IsCrouch"; // Bool
		private readonly int PARAM_ISCROUCH_ID = Animator.StringToHash(PARAM_ISCROUCH);
		private const string PARAM_TURNVALUE = "TurnValue"; // Float
		private readonly int PARAM_TURNVALUE_ID = Animator.StringToHash(PARAM_TURNVALUE);
		private const string PARAM_TRIGGERTURN = "TriggerTurn"; // Trigger
		private readonly int PARAM_TRIGGERTURN_ID = Animator.StringToHash(PARAM_TRIGGERTURN);
		private const string PARAM_LOCOMOTIONTYPE = "LocomotionType"; // Int
		private readonly int PARAM_LOCOMOTIONTYPE_ID = Animator.StringToHash(PARAM_LOCOMOTIONTYPE);
		private const string PARAM_AIMRIGHT = "AimRight"; // Float
		private readonly int PARAM_AIMRIGHT_ID = Animator.StringToHash(PARAM_AIMRIGHT);
		private const string PARAM_AIMUP = "AimUp"; // Float
		private readonly int PARAM_AIMUP_ID = Animator.StringToHash(PARAM_AIMUP);

		// States & Layer IDs
		// Layer 0 - Base
		private readonly int LAYER_0 = 0;
		private const string L_0__DEFAULT_STAND_LOCOMOTION = "DefaultStandLocomotion";
		private readonly int L_0__DEFAULT_STAND_LOCOMOTION_ID = Animator.StringToHash(L_0__DEFAULT_STAND_LOCOMOTION);
		private const string L_0__DEFAULT_CROUCH_LOCOMOTION = "DefaultCrouchLocomotion";
		private readonly int L_0__DEFAULT_CROUCH_LOCOMOTION_ID = Animator.StringToHash(L_0__DEFAULT_CROUCH_LOCOMOTION);
		private const string L_0__RIFLE_CROUCH_LOCOMOTION = "RifleCrouchLocomotion";
		private readonly int L_0__RIFLE_CROUCH_LOCOMOTION_ID = Animator.StringToHash(L_0__RIFLE_CROUCH_LOCOMOTION);
		private const string L_0__RIFLE_STAND_LOCOMOTION = "RifleStandLocomotion";
		private readonly int L_0__RIFLE_STAND_LOCOMOTION_ID = Animator.StringToHash(L_0__RIFLE_STAND_LOCOMOTION);
		private const string L_0__DEFAULT_JUMP_START = "DefaultJumpStart";
		private readonly int L_0__DEFAULT_JUMP_START_ID = Animator.StringToHash(L_0__DEFAULT_JUMP_START);
		private const string L_0__DEFAULT_JUMP_LOOP = "DefaultJumpLoop";
		private readonly int L_0__DEFAULT_JUMP_LOOP_ID = Animator.StringToHash(L_0__DEFAULT_JUMP_LOOP);
		private const string L_0__RIFLE_JUMP_START = "RifleJumpStart";
		private readonly int L_0__RIFLE_JUMP_START_ID = Animator.StringToHash(L_0__RIFLE_JUMP_START);
		private const string L_0__DEFAULT_JUMP_END = "DefaultJumpEnd";
		private readonly int L_0__DEFAULT_JUMP_END_ID = Animator.StringToHash(L_0__DEFAULT_JUMP_END);
		private const string L_0__RIFLE_JUMP_LOOP = "RifleJumpLoop";
		private readonly int L_0__RIFLE_JUMP_LOOP_ID = Animator.StringToHash(L_0__RIFLE_JUMP_LOOP);
		private const string L_0__RIFLE_JUMP_END = "RifleJumpEnd";
		private readonly int L_0__RIFLE_JUMP_END_ID = Animator.StringToHash(L_0__RIFLE_JUMP_END);
		private const string L_0__SLIDE = "Slide";
		private readonly int L_0__SLIDE_ID = Animator.StringToHash(L_0__SLIDE);
		private const string L_0__SHOW_RIFLE = "ShowRifle";
		private readonly int L_0__SHOW_RIFLE_ID = Animator.StringToHash(L_0__SHOW_RIFLE);
		private const string L_0__HIDE_RIFLE = "HideRifle";
		private readonly int L_0__HIDE_RIFLE_ID = Animator.StringToHash(L_0__HIDE_RIFLE);
		private const string L_0__EMPTY = "Empty";
		private readonly int L_0__EMPTY_ID = Animator.StringToHash(L_0__EMPTY);
		private const string L_0__SHOW_PISTOL = "ShowPistol";
		private readonly int L_0__SHOW_PISTOL_ID = Animator.StringToHash(L_0__SHOW_PISTOL);
		private const string L_0__PISTOL_STAND_LOCOMOTION = "PistolStandLocomotion";
		private readonly int L_0__PISTOL_STAND_LOCOMOTION_ID = Animator.StringToHash(L_0__PISTOL_STAND_LOCOMOTION);
		private const string L_0__PISTOL_CROUCH_LOCOMOTION = "PistolCrouchLocomotion";
		private readonly int L_0__PISTOL_CROUCH_LOCOMOTION_ID = Animator.StringToHash(L_0__PISTOL_CROUCH_LOCOMOTION);
		private const string L_0__HIDE_PISTOL = "HidePistol";
		private readonly int L_0__HIDE_PISTOL_ID = Animator.StringToHash(L_0__HIDE_PISTOL);
		private const string L_0__PISTOL_JUMP_START = "PistolJumpStart";
		private readonly int L_0__PISTOL_JUMP_START_ID = Animator.StringToHash(L_0__PISTOL_JUMP_START);
		private const string L_0__PISTOL_JUMP_LOOP = "PistolJumpLoop";
		private readonly int L_0__PISTOL_JUMP_LOOP_ID = Animator.StringToHash(L_0__PISTOL_JUMP_LOOP);
		private const string L_0__PISTOL_JUMP_END = "PistolJumpEnd";
		private readonly int L_0__PISTOL_JUMP_END_ID = Animator.StringToHash(L_0__PISTOL_JUMP_END);
		private const string L_0__UNARMED_DEATH = "UnarmedDeath";
		private readonly int L_0__UNARMED_DEATH_ID = Animator.StringToHash(L_0__UNARMED_DEATH);
		private const string L_0__RIFLE_DEATH = "RifleDeath";
		private readonly int L_0__RIFLE_DEATH_ID = Animator.StringToHash(L_0__RIFLE_DEATH);
		private const string L_0__PISTOL_DEATH = "PistolDeath";
		private readonly int L_0__PISTOL_DEATH_ID = Animator.StringToHash(L_0__PISTOL_DEATH);

		// Layer 1 - Detail
		private readonly int LAYER_1 = 1;
		private const string L_1__EMPTY = "Empty";
		private readonly int L_1__EMPTY_ID = Animator.StringToHash(L_1__EMPTY);
		private const string L_1__DEFAULT_STAND_TURN = "DefaultStandTurn";
		private readonly int L_1__DEFAULT_STAND_TURN_ID = Animator.StringToHash(L_1__DEFAULT_STAND_TURN);
		private const string L_1__DEFAULT_CROUCH_TURN = "DefaultCrouchTurn";
		private readonly int L_1__DEFAULT_CROUCH_TURN_ID = Animator.StringToHash(L_1__DEFAULT_CROUCH_TURN);
		private const string L_1__STAND_IDLE = "StandIdle";
		private readonly int L_1__STAND_IDLE_ID = Animator.StringToHash(L_1__STAND_IDLE);
		private const string L_1__DEFAULT_CROUCH_2_STAND = "DefaultCrouch2Stand";
		private readonly int L_1__DEFAULT_CROUCH_2_STAND_ID = Animator.StringToHash(L_1__DEFAULT_CROUCH_2_STAND);
		private const string L_1__DEFAULT_STAND_2_CROUCH = "DefaultStand2Crouch";
		private readonly int L_1__DEFAULT_STAND_2_CROUCH_ID = Animator.StringToHash(L_1__DEFAULT_STAND_2_CROUCH);
		private const string L_1__CROUCH_IDLE = "CrouchIdle";
		private readonly int L_1__CROUCH_IDLE_ID = Animator.StringToHash(L_1__CROUCH_IDLE);
		private const string L_1__RIFLE_STAND_2_CROUCH = "RifleStand2Crouch";
		private readonly int L_1__RIFLE_STAND_2_CROUCH_ID = Animator.StringToHash(L_1__RIFLE_STAND_2_CROUCH);
		private const string L_1__RIFLE_CROUCH_2_STAND = "RifleCrouch2Stand";
		private readonly int L_1__RIFLE_CROUCH_2_STAND_ID = Animator.StringToHash(L_1__RIFLE_CROUCH_2_STAND);
		private const string L_1__RIFLE_IDLE = "RifleIdle";
		private readonly int L_1__RIFLE_IDLE_ID = Animator.StringToHash(L_1__RIFLE_IDLE);
		private const string L_1__RIFLE_CROUCH = "RifleCrouch";
		private readonly int L_1__RIFLE_CROUCH_ID = Animator.StringToHash(L_1__RIFLE_CROUCH);
		private const string L_1__RIFLE_STAND_TURN = "RifleStandTurn";
		private readonly int L_1__RIFLE_STAND_TURN_ID = Animator.StringToHash(L_1__RIFLE_STAND_TURN);
		private const string L_1__RIFLE_CROUCH_TURN = "RifleCrouchTurn";
		private readonly int L_1__RIFLE_CROUCH_TURN_ID = Animator.StringToHash(L_1__RIFLE_CROUCH_TURN);
		private const string L_1__PISTOL_STAND_2_CROUCH = "PistolStand2Crouch";
		private readonly int L_1__PISTOL_STAND_2_CROUCH_ID = Animator.StringToHash(L_1__PISTOL_STAND_2_CROUCH);
		private const string L_1__PISTOL_CROUCH_2_STAND = "PistolCrouch2Stand";
		private readonly int L_1__PISTOL_CROUCH_2_STAND_ID = Animator.StringToHash(L_1__PISTOL_CROUCH_2_STAND);
		private const string L_1__PISTOL_IDLE = "PistolIdle";
		private readonly int L_1__PISTOL_IDLE_ID = Animator.StringToHash(L_1__PISTOL_IDLE);
		private const string L_1__PISTOL_CROUCH = "PistolCrouch";
		private readonly int L_1__PISTOL_CROUCH_ID = Animator.StringToHash(L_1__PISTOL_CROUCH);
		private const string L_1__PISTOL_STAND_TURN = "PistolStandTurn";
		private readonly int L_1__PISTOL_STAND_TURN_ID = Animator.StringToHash(L_1__PISTOL_STAND_TURN);
		private const string L_1__PISTOL_CROUCH_TURN = "PistolCrouchTurn";
		private readonly int L_1__PISTOL_CROUCH_TURN_ID = Animator.StringToHash(L_1__PISTOL_CROUCH_TURN);

		// Layer 2 - Aim
		private readonly int LAYER_2 = 2;
		private const string L_2__RIFLE_STAND_AIM = "RifleStandAim";
		private readonly int L_2__RIFLE_STAND_AIM_ID = Animator.StringToHash(L_2__RIFLE_STAND_AIM);
		private const string L_2__RIFLE_CROUCH_AIM = "RifleCrouchAim";
		private readonly int L_2__RIFLE_CROUCH_AIM_ID = Animator.StringToHash(L_2__RIFLE_CROUCH_AIM);
		private const string L_2__EMPTY = "Empty";
		private readonly int L_2__EMPTY_ID = Animator.StringToHash(L_2__EMPTY);
		private const string L_2__PISTOL_STAND_AIM = "PistolStandAim";
		private readonly int L_2__PISTOL_STAND_AIM_ID = Animator.StringToHash(L_2__PISTOL_STAND_AIM);
		private const string L_2__PISTOL_CROUCH_AIM = "PistolCrouchAim";
		private readonly int L_2__PISTOL_CROUCH_AIM_ID = Animator.StringToHash(L_2__PISTOL_CROUCH_AIM);

		// Layer 3 - Upper
		private readonly int LAYER_3 = 3;
		private const string L_3__SHOW_RIFLE = "ShowRifle";
		private readonly int L_3__SHOW_RIFLE_ID = Animator.StringToHash(L_3__SHOW_RIFLE);
		private const string L_3__EMPTY = "Empty";
		private readonly int L_3__EMPTY_ID = Animator.StringToHash(L_3__EMPTY);
		private const string L_3__HIDE_RIFLE = "HideRifle";
		private readonly int L_3__HIDE_RIFLE_ID = Animator.StringToHash(L_3__HIDE_RIFLE);
		private const string L_3__RIFLE_RELOAD = "RifleReload";
		private readonly int L_3__RIFLE_RELOAD_ID = Animator.StringToHash(L_3__RIFLE_RELOAD);
		private const string L_3__RIFLE_SHOOT = "RifleShoot";
		private readonly int L_3__RIFLE_SHOOT_ID = Animator.StringToHash(L_3__RIFLE_SHOOT);
		private const string L_3__HIDE_PISTOL = "HidePistol";
		private readonly int L_3__HIDE_PISTOL_ID = Animator.StringToHash(L_3__HIDE_PISTOL);
		private const string L_3__SHOW_PISTOL = "ShowPistol";
		private readonly int L_3__SHOW_PISTOL_ID = Animator.StringToHash(L_3__SHOW_PISTOL);
		private const string L_3__PISTOL_RELOAD = "PistolReload";
		private readonly int L_3__PISTOL_RELOAD_ID = Animator.StringToHash(L_3__PISTOL_RELOAD);
		private const string L_3__PISTOL_SHOOT = "PistolShoot";
		private readonly int L_3__PISTOL_SHOOT_ID = Animator.StringToHash(L_3__PISTOL_SHOOT);


		// Play Animations Layer 0
		public void PlayDefaultStandLocomotion(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_0__DEFAULT_STAND_LOCOMOTION_ID, fixedTransitionDuration, 0);
		}
		public void PlayDefaultCrouchLocomotion(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_0__DEFAULT_CROUCH_LOCOMOTION_ID, fixedTransitionDuration, 0);
		}
		public void PlayRifleCrouchLocomotion(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_0__RIFLE_CROUCH_LOCOMOTION_ID, fixedTransitionDuration, 0);
		}
		public void PlayRifleStandLocomotion(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_0__RIFLE_STAND_LOCOMOTION_ID, fixedTransitionDuration, 0);
		}
		public void PlayDefaultJumpStart(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_0__DEFAULT_JUMP_START_ID, fixedTransitionDuration, 0);
		}
		public void PlayDefaultJumpLoop(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_0__DEFAULT_JUMP_LOOP_ID, fixedTransitionDuration, 0);
		}
		public void PlayRifleJumpStart(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_0__RIFLE_JUMP_START_ID, fixedTransitionDuration, 0);
		}
		public void PlayDefaultJumpEnd(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_0__DEFAULT_JUMP_END_ID, fixedTransitionDuration, 0);
		}
		public void PlayRifleJumpLoop(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_0__RIFLE_JUMP_LOOP_ID, fixedTransitionDuration, 0);
		}
		public void PlayRifleJumpEnd(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_0__RIFLE_JUMP_END_ID, fixedTransitionDuration, 0);
		}
		public void PlaySlide(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_0__SLIDE_ID, fixedTransitionDuration, 0);
		}
		public void PlayShowRifle(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_0__SHOW_RIFLE_ID, fixedTransitionDuration, 0);
		}
		public void PlayHideRifle(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_0__HIDE_RIFLE_ID, fixedTransitionDuration, 0);
		}
		public void PlayEmpty(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_0__EMPTY_ID, fixedTransitionDuration, 0);
		}
		public void PlayShowPistol(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_0__SHOW_PISTOL_ID, fixedTransitionDuration, 0);
		}
		public void PlayPistolStandLocomotion(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_0__PISTOL_STAND_LOCOMOTION_ID, fixedTransitionDuration, 0);
		}
		public void PlayPistolCrouchLocomotion(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_0__PISTOL_CROUCH_LOCOMOTION_ID, fixedTransitionDuration, 0);
		}
		public void PlayHidePistol(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_0__HIDE_PISTOL_ID, fixedTransitionDuration, 0);
		}
		public void PlayPistolJumpStart(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_0__PISTOL_JUMP_START_ID, fixedTransitionDuration, 0);
		}
		public void PlayPistolJumpLoop(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_0__PISTOL_JUMP_LOOP_ID, fixedTransitionDuration, 0);
		}
		public void PlayPistolJumpEnd(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_0__PISTOL_JUMP_END_ID, fixedTransitionDuration, 0);
		}
		public void PlayUnarmedDeath(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_0__UNARMED_DEATH_ID, fixedTransitionDuration, 0);
		}
		public void PlayRifleDeath(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_0__RIFLE_DEATH_ID, fixedTransitionDuration, 0);
		}
		public void PlayPistolDeath(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_0__PISTOL_DEATH_ID, fixedTransitionDuration, 0);
		}
		// Play Animations Layer 1
		public void PlayEmpty_1(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_1__EMPTY_ID, fixedTransitionDuration, 1);
		}
		public void PlayDefaultStandTurn_1(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_1__DEFAULT_STAND_TURN_ID, fixedTransitionDuration, 1);
		}
		public void PlayDefaultCrouchTurn_1(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_1__DEFAULT_CROUCH_TURN_ID, fixedTransitionDuration, 1);
		}
		public void PlayStandIdle_1(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_1__STAND_IDLE_ID, fixedTransitionDuration, 1);
		}
		public void PlayDefaultCrouch2Stand_1(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_1__DEFAULT_CROUCH_2_STAND_ID, fixedTransitionDuration, 1);
		}
		public void PlayDefaultStand2Crouch_1(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_1__DEFAULT_STAND_2_CROUCH_ID, fixedTransitionDuration, 1);
		}
		public void PlayCrouchIdle_1(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_1__CROUCH_IDLE_ID, fixedTransitionDuration, 1);
		}
		public void PlayRifleStand2Crouch_1(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_1__RIFLE_STAND_2_CROUCH_ID, fixedTransitionDuration, 1);
		}
		public void PlayRifleCrouch2Stand_1(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_1__RIFLE_CROUCH_2_STAND_ID, fixedTransitionDuration, 1);
		}
		public void PlayRifleIdle_1(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_1__RIFLE_IDLE_ID, fixedTransitionDuration, 1);
		}
		public void PlayRifleCrouch_1(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_1__RIFLE_CROUCH_ID, fixedTransitionDuration, 1);
		}
		public void PlayRifleStandTurn_1(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_1__RIFLE_STAND_TURN_ID, fixedTransitionDuration, 1);
		}
		public void PlayRifleCrouchTurn_1(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_1__RIFLE_CROUCH_TURN_ID, fixedTransitionDuration, 1);
		}
		public void PlayPistolStand2Crouch_1(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_1__PISTOL_STAND_2_CROUCH_ID, fixedTransitionDuration, 1);
		}
		public void PlayPistolCrouch2Stand_1(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_1__PISTOL_CROUCH_2_STAND_ID, fixedTransitionDuration, 1);
		}
		public void PlayPistolIdle_1(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_1__PISTOL_IDLE_ID, fixedTransitionDuration, 1);
		}
		public void PlayPistolCrouch_1(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_1__PISTOL_CROUCH_ID, fixedTransitionDuration, 1);
		}
		public void PlayPistolStandTurn_1(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_1__PISTOL_STAND_TURN_ID, fixedTransitionDuration, 1);
		}
		public void PlayPistolCrouchTurn_1(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_1__PISTOL_CROUCH_TURN_ID, fixedTransitionDuration, 1);
		}
		// Play Animations Layer 2
		public void PlayRifleStandAim_2(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_2__RIFLE_STAND_AIM_ID, fixedTransitionDuration, 2);
		}
		public void PlayRifleCrouchAim_2(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_2__RIFLE_CROUCH_AIM_ID, fixedTransitionDuration, 2);
		}
		public void PlayEmpty_2(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_2__EMPTY_ID, fixedTransitionDuration, 2);
		}
		public void PlayPistolStandAim_2(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_2__PISTOL_STAND_AIM_ID, fixedTransitionDuration, 2);
		}
		public void PlayPistolCrouchAim_2(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_2__PISTOL_CROUCH_AIM_ID, fixedTransitionDuration, 2);
		}
		// Play Animations Layer 3
		public void PlayShowRifle_3(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_3__SHOW_RIFLE_ID, fixedTransitionDuration, 3);
		}
		public void PlayEmpty_3(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_3__EMPTY_ID, fixedTransitionDuration, 3);
		}
		public void PlayHideRifle_3(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_3__HIDE_RIFLE_ID, fixedTransitionDuration, 3);
		}
		public void PlayRifleReload_3(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_3__RIFLE_RELOAD_ID, fixedTransitionDuration, 3);
		}
		public void PlayRifleShoot_3(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_3__RIFLE_SHOOT_ID, fixedTransitionDuration, 3);
		}
		public void PlayHidePistol_3(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_3__HIDE_PISTOL_ID, fixedTransitionDuration, 3);
		}
		public void PlayShowPistol_3(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_3__SHOW_PISTOL_ID, fixedTransitionDuration, 3);
		}
		public void PlayPistolReload_3(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_3__PISTOL_RELOAD_ID, fixedTransitionDuration, 3);
		}
		public void PlayPistolShoot_3(float fixedTransitionDuration)
		{
			m_Animator.CrossFadeInFixedTime(L_3__PISTOL_SHOOT_ID, fixedTransitionDuration, 3);
		}
		#endregion

		
		public Animator m_Animator;
		public List<SkinnedMeshRenderer> m_ArmMeshList;
		public List<SkinnedMeshRenderer> m_HeadMeshList;

	
		#region Hashes

		private int[] LocomotionStateHashes;
		private int[] CrouchStateHashes;
		private int[] JumpEndStateHashes;
		private int[] ShowHideWeaponStateHashes;
		private int[] SwitchCrouchStateHashes;
		private int[] ShootStateHashes;
		private int[] ReloadStateHashes;

		private void InitStatesHashID()
		{
			LocomotionStateHashes = new int[]
			{
				L_0__DEFAULT_STAND_LOCOMOTION_ID,
				L_0__DEFAULT_CROUCH_LOCOMOTION_ID,
				L_0__RIFLE_CROUCH_LOCOMOTION_ID,
				L_0__RIFLE_STAND_LOCOMOTION_ID
			};

			CrouchStateHashes = new int[]
			{
				L_0__DEFAULT_CROUCH_LOCOMOTION_ID,
				L_0__PISTOL_CROUCH_LOCOMOTION_ID,
				L_0__RIFLE_CROUCH_LOCOMOTION_ID
			};

			JumpEndStateHashes = new int[]
			{
				L_0__DEFAULT_JUMP_END_ID,
				L_0__RIFLE_JUMP_END_ID,
				L_0__PISTOL_JUMP_END_ID
			};

			ShowHideWeaponStateHashes = new int[]
			{
				L_0__SHOW_RIFLE_ID,
				L_0__HIDE_RIFLE_ID,
				L_0__SHOW_PISTOL_ID,
				L_0__HIDE_PISTOL_ID
			};

			SwitchCrouchStateHashes = new int[]
			{
				L_1__DEFAULT_CROUCH_2_STAND_ID,
				L_1__DEFAULT_STAND_2_CROUCH_ID,
				L_1__RIFLE_STAND_2_CROUCH_ID,
				L_1__RIFLE_CROUCH_2_STAND_ID,
				L_1__PISTOL_STAND_2_CROUCH_ID,
				L_1__PISTOL_CROUCH_2_STAND_ID
			};

			ShootStateHashes = new int[]
			{
				L_3__RIFLE_SHOOT_ID, L_3__PISTOL_SHOOT_ID
			};

			ReloadStateHashes = new int[]
			{
				L_3__RIFLE_RELOAD_ID, L_3__PISTOL_RELOAD_ID
			};
		}

		
		#endregion
		
		#region Stances

		public enum LocomotionType
		{
			Unarmed,
			Rifle,
			Pistol
		}
		
		public LocomotionType CurrentLocomotionType
		{
			get
			{
				return m_LocomotionType;
			}
		}
		private LocomotionType m_LocomotionType = LocomotionType.Unarmed;
		public enum LocomotionStance
		{
			Stand,
			Crouch
		}
		public LocomotionStance m_LocomotionStance = LocomotionStance.Stand;
		#endregion
		
		public CharacterIK m_CharacterIK;

		void Awake()
		{
			Init();
		}

		void Start()
		{
		}

		public void Init()
		{
			InitStatesHashID();
			if (m_Animator == null)
			{
				m_Animator = GetComponent<Animator>();
			}

			if (m_ArmMeshList == null)
			{
				m_ArmMeshList = new List<SkinnedMeshRenderer>();
			}

			if (m_HeadMeshList == null)
			{
				m_HeadMeshList = new List<SkinnedMeshRenderer>();
			}
		}

		// public void UpdateAnimationState(float speed, bool isGrounded, bool isCrouching, bool isSprinting)
		// {
		// 	if (m_Animator == null)
		// 		return;
		//
		// 	m_CurrentSpeed = Mathf.SmoothDamp(m_CurrentSpeed, speed, ref m_SpeedVelocity, m_SpeedSmoothTime);
		//
		// 	m_Animator.SetFloat(s_SpeedHash, m_CurrentSpeed);
		// 	m_Animator.SetBool(s_IsGroundedHash, isGrounded);
		// 	m_Animator.SetBool(s_IsCrouchingHash, isCrouching);
		// 	m_Animator.SetBool(s_IsSprintingHash, isSprinting);
		// }

		public void SetArmMeshVisible(bool visible)
		{
			foreach (var mesh in m_ArmMeshList)
			{
				if (mesh != null)
				{
					mesh.enabled = visible;
				}
			}
		}

		public void SetHeadMeshVisible(bool visible)
		{
			foreach (var mesh in m_HeadMeshList)
			{
				if (mesh != null)
				{
					mesh.enabled = visible;
				}
			}
		}

		public void SetFirstPersonView(bool isFirstPerson)
		{
			SetHeadMeshVisible(!isFirstPerson);
		}

		#region Layer Control

		private void EnableDetailLayer()
		{
			m_Animator.FadeLayerWeight(LAYER_1, 1f, 0.1f);
		}
		
		public void DisableDetailLayer(float duration = 0.2f)
		{
			m_Animator.FadeLayerWeight(LAYER_1, 0f, 0.1f);
			
			m_Animator.FadeLayerWeight(1, 0f, 0.25f, () => {
				PlayEmpty_1(0.1f); 
			});
		}

		#endregion
		
		#region Stance
		public void SetStanceCrouch()
		{
			m_LocomotionStance = LocomotionStance.Crouch;
		}

		public void SetStanceStand()
		{
			m_LocomotionStance = LocomotionStance.Stand;
		}
		
		public void Play2Crouch(bool playDetail, bool isOnGround)
		{
			SetStanceCrouch();
			// if (playDetail)
			// {
			// 	DisableDetailLayer();
			// 	SetTurnTagOffOnly();
			// 	// if (m_CanPlaySwitchCrouchStand)
			// 	// {
			// 	// 	EnableDetailLayer();
			// 	// 	PlayDetailStand2Crouch();
			// 	// }
			// }

			float fade = 0.2f;
			if (!isOnGround)
			{
				fade = 0.01f;
			}
			if (m_LocomotionType == LocomotionType.Unarmed)
			{
				PlayLocomotionFixedTime(L_0__DEFAULT_CROUCH_LOCOMOTION_ID, fade);
			}
			else if (m_LocomotionType == LocomotionType.Rifle)
			{
				PlayLocomotionFixedTime(L_0__RIFLE_CROUCH_LOCOMOTION_ID, fade);
				//PlayRifleCrouchAim();
			}
			else if (m_LocomotionType == LocomotionType.Pistol)
			{
				PlayLocomotionFixedTime(L_0__PISTOL_CROUCH_LOCOMOTION_ID, fade);
				//PlayPistolCrouchAim();
			}
		}
		
		public void Play2Stand(bool playDetail)
		{
			SetStanceStand();
			// if (playDetail)
			// {
			// 	DisableDetailLayer();
			// 	SetTurnTagOffOnly();
			// 	// if (m_CanPlaySwitchCrouchStand)
			// 	// {
			// 	// 	EnableDetailLayer();
			// 	// 	PlayDetailCrouch2Stand();
			// 	// }
			// }

			if (m_LocomotionType == LocomotionType.Unarmed)
			{
				PlayLocomotionFixedTime(L_0__DEFAULT_STAND_LOCOMOTION_ID, 0.2f);
			}
			else if (m_LocomotionType == LocomotionType.Rifle)
			{
				PlayLocomotionFixedTime(L_0__RIFLE_STAND_LOCOMOTION_ID, 0.2f);
				//PlayRifleStandAim();
			}
			else if (m_LocomotionType == LocomotionType.Pistol)
			{
				PlayLocomotionFixedTime(L_0__PISTOL_STAND_LOCOMOTION_ID, 0.2f);
				//PlayPistolStandAim();
			}
		}
		#endregion

		#region Turn

		private bool m_PlayingTurnLeft;
		private bool m_PlayingTurnRight;

		public void SetTurnTagOff()
		{
			if (m_PlayingTurnLeft || m_PlayingTurnRight)
			{
				DisableDetailLayer();
			}
			m_PlayingTurnRight = false;
			m_PlayingTurnLeft = false;
		}
		
		public void SetTurnTagOffOnly()
		{
			m_PlayingTurnLeft = false;
			m_PlayingTurnRight = false;
		}
		
		public bool PlayTurn(float angle)
		{
			// if (IsPlayingSwitchCrouchStand())
			// 	//if (m_DetailPlayingSwitchCrouchStand)
			// {
			// 	return false;
			// }
			//
			EnableDetailLayer();
			
			//check if already playing turn in same direction
			if ((angle > 0f && m_PlayingTurnRight)
			    || (angle < 0f && m_PlayingTurnLeft))
			{
				//Debug.Log("already playing turn in same direction");
				return false;
			}
			
			if (m_LocomotionStance == LocomotionStance.Stand)
			{
				PlayStandTurn(angle);
			}
			else if (m_LocomotionStance == LocomotionStance.Crouch)
			{
				PlayCrouchTurn(angle);
			}
			
			m_PlayingTurnLeft = angle < 0f;
			m_PlayingTurnRight = angle > 0f;
			return true;
		}
		
		public void PlayStandTurn(float value)
		{
			value /= 90f;
			value = Mathf.Clamp(value, -1f, 1f);
			m_Animator.SetFloat(PARAM_TURNVALUE_ID, value);
			m_Animator.SetBool(PARAM_ISCROUCH_ID, false);
			m_Animator.SetTrigger(PARAM_TRIGGERTURN_ID);
		}

		public void PlayCrouchTurn(float value)
		{
			value /= 90f;
			value = Mathf.Clamp(value, -1f, 1f);
			m_Animator.SetFloat(PARAM_TURNVALUE_ID, value);
			m_Animator.SetBool(PARAM_ISCROUCH_ID, true);
			m_Animator.SetTrigger(PARAM_TRIGGERTURN_ID);
		}

		#endregion
		
		#region Locomotion
		
		[Range(0, 1)]
		public float m_LocomotionTransNormalized = 0.1f;
		
		public bool CheckPlayingLocomotion()
		{
			AnimatorStateInfo curState = m_Animator.GetCurrentAnimatorStateInfo(LAYER_0);
			for (int i = 0;i < LocomotionStateHashes.Length; i ++)
			{
				//if (curState.IsName(LocomotionStateHashes[i]))
				if (curState.shortNameHash == LocomotionStateHashes[i])
				{
					return true;
				}
			}
			return false;
		}
		
		public float GetCurrentLocomotionNormalizedTime(out bool prevIsLocomotion)
		{
			if (CheckPlayingLocomotion())
			{
				prevIsLocomotion = true;
				AnimatorStateInfo curState = m_Animator.GetCurrentAnimatorStateInfo(LAYER_0);
				return curState.normalizedTime;
			}
			prevIsLocomotion = false;
			return 0f;
		}
		
		public void PlayLocomotionFixedTime(int hashID, float fade)
		{
			bool prevLocomotion = false;
			float normalizedTime = GetCurrentLocomotionNormalizedTime(out prevLocomotion);
			normalizedTime += m_LocomotionTransNormalized;
			normalizedTime %= 1f;
			m_Animator.CrossFadeInFixedTime(hashID, fade, LAYER_0, normalizedTime);
		}

		//public void PlayLocomotion(string stateName, float fade)
		public void PlayLocomotion(int hashID, float fade)
		{
			bool prevLocomotion = false;
			float normalizedTime = GetCurrentLocomotionNormalizedTime(out prevLocomotion);
			normalizedTime += m_LocomotionTransNormalized;
			normalizedTime %= 1f;
			if (prevLocomotion)
			{
				m_Animator.CrossFade(hashID, fade, LAYER_0, normalizedTime);
			}
			else
			{
				m_Animator.CrossFadeInFixedTime(hashID, fade, LAYER_0, normalizedTime);
			}
		}
		#endregion
	}
}
