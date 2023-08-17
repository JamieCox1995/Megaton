using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraneHook : MonoBehaviour
{
    private LineRenderer m_line;
    [SerializeField]
    private Transform trolley;
    [SerializeField]
    private Transform hook;
	
	// Update is called once per frame
	void Update ()
    {
        if (m_line == null)
        {
            m_line = GetComponent<LineRenderer>();
        }

        else
        {
            m_line.SetPosition(0, trolley.position);
            m_line.SetPosition(1, hook.position);
        }
    }

    public void SetUpLine(Transform t, Transform h)
    {
        m_line = GetComponent<LineRenderer>();

        hook = h;
        trolley = t;

        m_line.SetPosition(0, trolley.position);
        m_line.SetPosition(1, hook.position);

    }
}
