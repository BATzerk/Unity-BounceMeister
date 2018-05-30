using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Collidable : MonoBehaviour {
	abstract public void OnCollideWithPlayer(Player player);
}
