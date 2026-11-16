# SWE402 Assignment 3 – Improving Game Feel

## Student
Name: Mohammed Fahad Alajmi  
Student ID: 202065020

## Run
- **Unity version:** 6000.3.27f1 (Unity 6.3; course target 6000.3.2f1, same 6.3 line)
- **Scenes:**
  - `Assets/_Project/Scenes/Before.unity` – original starter, unchanged
  - `Assets/_Project/Scenes/After.unity` – same gameplay with all feedback added
- **Controls:** move the paddle with the mouse, A/D, or Left/Right arrows. The ball launches automatically.
- **How to run:** add the project folder in Unity Hub, open a scene above, set the Game view to 16:9, press Play. Stop and restart Play for a fresh board.

## Intended feel
**Playful and bouncy.** Hits should feel springy and rewarding, like a toy: things squash and wobble back, bricks pop with a bright chime that climbs in pitch during a combo, and the score "bounces" when it grows, while the ball always stays easy to read.

## Improvements
All feedback is new code in `Assets/_Project/Scripts/Feel/` and is only placed in `After.unity`. No shared gameplay script, prefab, or material was modified. Every script subscribes to `GameEvents` in `OnEnable` and unsubscribes in `OnDisable`.

| # | Improvement | Trigger | Implementation file / object | Purpose |
|---|---|---|---|---|
| 1 | Brick impact particles | `GameEvents.BrickDestroyed` | `BrickBurstFX.cs` on the `BrickBurstFX` Particle System | A burst in the brick's own color at the brick's center makes each break visible and rewarding. One reused system with `Emit()` and zero emission rate, so bursts never pile up. |
| 2 | Squash & stretch + recoil | `BallHitPaddle` (paddle & ball), `BallHitWall` and `BrickDestroyed` (ball) | `SquashStretch.cs` on `Paddle/Visual` (Mode Paddle) and `Ball/Visual` (Mode Ball) | Paddle flattens and dips like a trampoline; ball flattens along the impact direction. Only the visual child is animated, so colliders and physics are unchanged. A damped spring *sets* (never adds) the deformation, so repeated hits cannot accumulate. |
| 3 | Event-based audio | `BallHitPaddle`, `BrickDestroyed`, `BallLost` (+ quiet `BallHitWall`) | `SfxPlayer.cs` on the `Audio` object; clips in `Assets/_Project/Audio/` | Distinct sounds for paddle "boing", brick "pop", and a descending "wah" on ball loss. Brick pitch rises with `ImpactInfo.Combo`. Each sound has its own AudioSource, slight pitch jitter, and a cooldown so repeats stay balanced. |
| 4 | Score punch | `GameEvents.ScoreChanged` (only when score increases) | `ScorePunch.cs` on `Canvas/ScoreText` | Score pops (with a small wobble) and flashes gold, slightly bigger during combos, then returns exactly to normal. It only scales/recolors the text object and never writes the text, so `HUD` keeps ownership of the score value (no `ScoreHandledExternally` needed). |
| 5 | Ball trail | Continuous while moving; cleared on `GameEvents.BallLost` | Trail Renderer + `TrailResetter.cs` on `Ball/Trail` | A short, tapered green trail makes the ball's speed and direction easier to read. `TrailResetter` hides and clears the trail when the ball teleports back after a miss, so no streak is drawn across the screen. |

**Extra:** `LifeLostCue.cs` on `Canvas/LivesText` shakes the LIVES text and flashes it red on `BallLost`, so ball loss is clear even with audio muted. It uses `BallLost` instead of `LivesChanged`, because losing the last life resets lives *upward* to 3.

**Combined techniques on one event:** brick destruction = particles + sound + score punch; ball loss = sound + LIVES shake/flash + trail reset.

### Key tuning values
- Paddle squash: amount 0.3, recoil 0.12, stiffness 350, damping 14
- Ball squash: amount 0.25, stiffness 400, damping 16
- Score punch: 0.35 (+0.05 per combo step, max 5), 0.3 s, gold flash
- Trail: time 0.18 s, width 0.4 → 0
- Audio volumes: paddle 0.6, brick 0.7, ball lost 0.8, wall 0.25

### Verification
- Rapid brick/wall hits: no permanent stretching, no particle pile-up, audio stays balanced, score always correct.
- Paddle parent scale stays (3, 0.5, 1) during play; only `Visual` children are animated.
- Losing the last life resets the score to 0 with no score pop; the LIVES cue still plays.
- With audio muted, brick destruction (particles + score pop) and ball loss (LIVES shake + count) are still clearly visible.
- `Before.unity` still shows the original, unpolished behavior.

## Playtest and revision
One person played Before and then After.
- **More satisfying:** After. The tester especially liked the brick destruction sound.
- **Clearer feedback:** After, but the tester noted the **ball and paddle colors were too similar**, which made the ball harder to track.
- **Distracting:** none mentioned.
- **Sound timing:** when two bricks broke close together, the tester said it "would feel more satisfying if the brick destruction sound played faster."

**Revisions:**
1. **Ball clarity:** changed the ball sprite and trail color to green (in `After` only) so the ball stands out from the white paddle.
2. **Brick sound speed:** Kept the brick cooldown low (0.01 s) so every break gets its own sound. 

**Result:** close brick hits now sound like quick, separate pops that keep up with the action, and the green ball is easy to follow against the paddle and bricks.

## AI use
- **Tool:** Claude (Anthropic).
- **What it helped produce:** The C# feedback scripts in `Scripts/Feel/` (`BrickBurstFX`, `SfxPlayer`, `SquashStretch`, `ScorePunch`, `TrailResetter`, `LifeLostCue`); the sound effects (synthesized with a Python script); Git troubleshooting; and a first draft of this README.
- **How I tested/revised it:** I created and wired every component in Unity myself, played the After scene repeatedly (rapid hits, deliberate ball losses, losing all lives, audio muted), checked that the Paddle/Ball physics objects never change scale, confirmed Before stays unchanged, and tuned values by ear and eye. After the playtest I changed the ball/trail color and the brick sound clip based on the tester's feedback.

## Credits
- Starter adapted from [JuiceLab](https://github.com/serkoceyhan/JuiceLab) by Serkan Ceyhan, MIT License. See `LICENSE` and `SOURCE.md`.
- Sound effects (`sfx_paddle`, `sfx_brick`, `sfx_brick_snappy`, `sfx_ball_lost`, `sfx_wall`): procedurally synthesized for this project with a Python script (AI-assisted); no third-party audio.