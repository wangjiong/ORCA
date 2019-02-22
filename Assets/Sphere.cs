using RVO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sphere : MonoBehaviour {
    public bool Debug = false;
    // Gizmos
    public List<Line> msGizmosLines = new List<Line>();
    public int msId;
    public RVO.Vector2 msVelocity;

    void OnDrawGizmos() {
        if (!Debug) {
            return;
        }
        Gizmos.color = Color.red;

        // ORCA
        Vector3 from;
        Vector3 to;
        foreach (Line msGizmosLine in msGizmosLines) {
            RVO.Vector2 from_ = msGizmosLine.point - msGizmosLine.direction * 100;
            RVO.Vector2 to_ = msGizmosLine.point + msGizmosLine.direction * 100;

            from = transform.position + new Vector3(from_.x(), 0, from_.y());
            to = transform.position + new Vector3(to_.x(), 0, to_.y());

            Gizmos.DrawLine(from, to);
        }
        msGizmosLines.Clear();
        // velocity
        Gizmos.color = Color.green;

        from = transform.position;
        to = transform.position + new Vector3(msVelocity.x(), 0, msVelocity.y());

        Gizmos.DrawLine(from, to);
    }
}
