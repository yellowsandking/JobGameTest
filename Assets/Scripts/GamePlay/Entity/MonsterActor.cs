using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterActor : ActorBase
{
	IAI m_AI = null;
	public override void OnInit()
	{
		m_AI = new AI();
		m_AI.Init(this);
	}
	public override void Update()
	{
		m_AI.LogicUpdate();
	}
}
