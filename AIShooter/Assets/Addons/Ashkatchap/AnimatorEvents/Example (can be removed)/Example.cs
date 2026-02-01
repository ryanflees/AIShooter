using UnityEngine;

public class Example : MonoBehaviour {
	public Animator animator;

	void OnGUI () {
		if (GUILayout.Button("Go to Relax")) {
			animator.SetBool("goToRelax", true);
		}

		foreach (var elem in animator.parameters) {
			string value = null;
			switch (elem.type) {
				case AnimatorControllerParameterType.Bool:
				case AnimatorControllerParameterType.Trigger:
					value = animator.GetBool(elem.name).ToString();
					break;
				case AnimatorControllerParameterType.Float:
					value = animator.GetFloat(elem.name).ToString();
					break;
				case AnimatorControllerParameterType.Int:
					value = animator.GetInteger(elem.name).ToString();
					break;
			}
			GUILayout.Label("Parameter " + elem.name + ": " + value);
		}
	}

	public void DeliberatelyThrowException() {
		throw new System.Exception("Exception thrown from user code.");
	}
}
