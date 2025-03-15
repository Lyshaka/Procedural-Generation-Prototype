using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
	[SerializeField] GameObject debugSprite;
	[SerializeField] float smoothTime = 0.3f;
	//[SerializeField] float immobileTotalTime = 2f;
	//[SerializeField] float movingSmoothTime = 1f;
	//[SerializeField] float immobileSmoothTime = 0.3f;

	Vector3 _currentVelocity;
	Vector3 _targetPosition;
	//Vector3 _currentTargetVelocity;
	//Vector3 _targetTargetPosition;

	//float _immobileCurrentTime;
	//bool _immobile;

	// Update is called once per frame
	void LateUpdate()
	{
		_targetPosition = Player.Instance.transform.position;
		_targetPosition.z = -10f;
		Camera.main.transform.position = Vector3.SmoothDamp(Camera.main.transform.position, _targetPosition, ref _currentVelocity, smoothTime);


		//if (Player.Instance.Velocity.sqrMagnitude > 0.01f)
		//{
		//	_immobile = false;
		//	_immobileCurrentTime = 0f;
		//	_targetTargetPosition = Player.Instance.transform.position + (Vector3)Player.Instance.Velocity * 0.25f;
		//}
		//else
		//{
		//	if (_immobileCurrentTime < immobileTotalTime)
		//	{
		//		_immobileCurrentTime += Time.deltaTime;
		//	}
		//	else
		//	{
		//		_immobile = true;
		//		_targetTargetPosition = Player.Instance.transform.position;
		//	}
		//}

		//_targetPosition = Vector3.SmoothDamp(_targetPosition, _targetTargetPosition, ref _currentTargetVelocity, 0.1f);

		//_targetPosition.z = 0f;
		//debugSprite.transform.position = _targetPosition;

		//_targetPosition.z = -10f;

		//Camera.main.transform.position = Vector3.SmoothDamp(Camera.main.transform.position, _targetPosition, ref _currentVelocity, _immobile ? immobileSmoothTime : movingSmoothTime);
	}
}
