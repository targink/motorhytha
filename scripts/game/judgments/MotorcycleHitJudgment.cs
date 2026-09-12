using System;
using System.Collections.Generic;

// Motorcycle mode's hit detection: a gate (an old map's Note, reused as-is)
// is graded once the bike reaches its timestamp, based on how close it was
// (both in time and lane) - Perfect/Good/Miss, matching Rhythia's own
// HIT_WINDOW for the boundary. Also drives a simple health bar: misses hurt
// more the longer your current streak of misses is (same shape as the
// framework's HealthJudgment.defaultHealthResult), hitting 0 fails the run.
public class MotorcycleHitJudgment
{
    private const double PerfectWindowMs = 20;

    private double healthStep = 15;

    public void ProcessGates(Attempt attempt, IList<Note> gates)
    {
        if (attempt.IsFailed)
        {
            return;
        }

        foreach (Note gate in gates)
        {
            if (gate.Hit)
            {
                continue;
            }

            double msUntilHit = gate.Millisecond - attempt.Progress;
            bool inLane = MotorcycleLanes.LaneFromNoteX(gate.X) == attempt.BikeLane;

            if (Math.Abs(msUntilHit) <= Constants.HIT_WINDOW && inLane)
            {
                gate.Hit = true;
                bool perfect = Math.Abs(msUntilHit) <= PerfectWindowMs;
                resolve(attempt, hit: true, grade: perfect ? "Perfect" : "Good", points: perfect ? 300 : 100);
            }
            else if (msUntilHit < -Constants.HIT_WINDOW)
            {
                gate.Hit = true; // missed - resolved so it stops being checked/rendered
                resolve(attempt, hit: false, grade: "Miss", points: 0);
            }
        }
    }

    private void resolve(Attempt attempt, bool hit, string grade, int points)
    {
        attempt.LastHitGrade = grade;

        if (hit)
        {
            attempt.Combo++;
            attempt.Score += points * attempt.Combo;
            healthStep = Math.Max(healthStep / 1.45, 15);
            attempt.Health = Math.Min(100, attempt.Health + healthStep / 1.75);
        }
        else
        {
            attempt.Combo = 0;
            attempt.Health = Math.Max(0, attempt.Health - healthStep);
            healthStep = Math.Min(healthStep * 1.2, 100);
        }

        if (attempt.Health <= 0)
        {
            attempt.IsFailed = true;
        }
    }
}
