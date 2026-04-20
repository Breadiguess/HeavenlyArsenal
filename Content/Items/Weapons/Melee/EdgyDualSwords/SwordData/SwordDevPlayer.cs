namespace HeavenlyArsenal.Content.Items.Weapons.Melee.EdgyDualSwords.SwordData
{
    internal class SwordDevPlayer : ModPlayer
    {

        // Debug condition toggle for now.
        public bool DebugConditionAlwaysTrue = true;

        // How long after a hit the music should remain "maintained".
        public const int HitGraceTime = 60; // 1 second at 60 FPS

        // Counts down after each valid hit.
        public int HitTimer;

        // Whether this music session is currently alive.
        public bool MusicSessionActive;

        // Whether the scene effect should still claim the music slot.
        // This remains true during fade-out so the song can continue from its current point.
        public bool KeepMusicAlive;

        // 0..1 fade multiplier.
        public float MusicFade;

        // How fast the music fades in/out.
        public const float FadeInSpeed = 0.08f;
        public const float FadeOutSpeed = 0.04f;

        // Tracks whether the session fully ended.
        // If true, the next hit starts from the beginning.
        public bool MusicFullyStopped = true;

        public override void ResetEffects()
        {
            // Put per-tick temporary conditions here later if needed.
            DebugConditionAlwaysTrue = true;
        }

        public override void PreUpdate()
        {
            // Countdown the hit timer.
            if (HitTimer > 0)
                HitTimer--;

            // If we are inside the grace window, the session is alive.
            if (HitTimer > 0)
            {
                MusicSessionActive = true;
                KeepMusicAlive = true;

                // Music is definitely not fully stopped if we're actively sustaining it.
                MusicFullyStopped = false;

                MusicFade = MathHelper.Clamp(MusicFade + FadeInSpeed, 0f, 1f);
            }
            else
            {
                // Grace period expired: begin fading out.
                if (KeepMusicAlive)
                {
                    MusicFade = MathHelper.Clamp(MusicFade - FadeOutSpeed, 0f, 1f);

                    // Once fully faded, release the session.
                    if (MusicFade <= 0f)
                    {
                        MusicFade = 0f;
                        MusicSessionActive = false;
                        KeepMusicAlive = false;
                        MusicFullyStopped = true;
                    }
                }
                else
                {
                    MusicSessionActive = false;
                    MusicFade = 0f;
                    MusicFullyStopped = true;
                }
            }
        }

        public void RegisterValidHit()
        {
            if (!DebugConditionAlwaysTrue)
                return;
           
            if (MusicFullyStopped)
            {
                Main.NewText("Fresh music start");
                MusicSessionActive = true;
                KeepMusicAlive = true;
                MusicFullyStopped = false;
                MusicFade = 0f;
            }
            else
            {
                Main.NewText("Music continued");
            }
            // Refresh the grace window.
            HitTimer = HitGraceTime;

            // If the music had fully died, this is a fresh start.
            // If it had not fully died, the same session continues naturally.
            if (MusicFullyStopped)
            {
                MusicSessionActive = true;
                KeepMusicAlive = true;
                MusicFullyStopped = false;

                // Optional:
                // set fade to 0 so it fades in from silence on first fresh hit,
                // or set to something like 0.25f for a snappier response.
                MusicFade = 0f;
            }
        }

    }
}
