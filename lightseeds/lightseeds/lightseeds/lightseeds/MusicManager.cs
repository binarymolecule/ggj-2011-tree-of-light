using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace lightseeds
{
  public class MusicManager
  {
    private float DarkThreshold;
    private float LightThreshold;
    private AudioEngine audioEngine;
    private WaveBank waveBank;
    private SoundBank soundBank;

    private int CurrentStage;
    private int NextStage;

    private AudioCategory light = new AudioCategory();
    private AudioCategory dark = new AudioCategory();
    private AudioCategory light2 = new AudioCategory();
    private AudioCategory dark2 = new AudioCategory();

    private float LightVolume = 1.0f;
    private float TargetLightVolume = 1.0f;
    private float DarkVolume = 0.0f;
    private float TargetDarkVolume = 0.0f;

    private void SetLightVolume(float value)
    {
      TargetLightVolume = value;
    }

    private void SetDarkVolume(float value)
    {
      TargetDarkVolume = value;
    }

    public MusicManager(float lightThreshold, float darkThreshold) 
    {
      LightThreshold = lightThreshold;
      DarkThreshold = darkThreshold;

      audioEngine = new AudioEngine("Content/audio/test.xgs");
      waveBank = new WaveBank(audioEngine, "Content/audio/Wave Bank1.xwb");
      soundBank = new SoundBank(audioEngine, "Content/audio/Sound Bank.xsb");

      light = audioEngine.GetCategory("Light");
      dark = audioEngine.GetCategory("Dark");
      light2 = audioEngine.GetCategory("Light2");
      dark2 = audioEngine.GetCategory("Dark2");

      CurrentStage = 0;
      NextStage = 0;
      light.SetVolume(1.0f);
      dark.SetVolume(0.0f);
      light2.SetVolume(0.0f);
      dark2.SetVolume(0.0f);
    }

    public void SetVolume(float DistanceToTree, float DistanceToDarkness)
    {
      if (DistanceToDarkness < DarkThreshold)
      {
        SetLightVolume(0.0f);
        SetDarkVolume(1.0f);
        return;
      }

      float TotalDistance = DistanceToDarkness + DistanceToTree;
      float TotalThreshold = LightThreshold + DarkThreshold;
      float TransitionSpace = TotalDistance - TotalThreshold;
      float MinDistance = 4*(TotalThreshold);
      float Over = MinDistance - TotalDistance;

      float l;
      float d;

      if (Over > 0.0f)
      {
        if(DistanceToTree + Over < LightThreshold)
        {
          SetLightVolume(1.0f);
          SetDarkVolume(0.0f);
          return;
        }

        l = (DistanceToTree - LightThreshold + Over) / (MinDistance - TotalThreshold);
        d = 1.0f - l;

        SetLightVolume(d);
        SetDarkVolume(l);
        return;
      }

      if (DistanceToTree < LightThreshold)
      {
        SetLightVolume(1.0f);
        SetDarkVolume(0.0f);
        return;
      }

      l = (DistanceToTree - LightThreshold) / (DistanceToTree + DistanceToDarkness - LightThreshold - DarkThreshold);
      d = 1.0f - l;

      SetLightVolume(d);
      SetDarkVolume(l);
    }

    private bool LoopStarted = false;
    public void StartLoop()
    {
      if (!LoopStarted)
      {
        soundBank.PlayCue("light");
        soundBank.PlayCue("dark");
        soundBank.PlayCue("light2");
        soundBank.PlayCue("dark2");
        LoopStarted = true;
      }
    }

    public void Play(string CueName)
    {
      soundBank.PlayCue(CueName);
    }

    private bool SecondStageStarted = false;
    public void SetNextStage(int stage)
    {
      if (!SecondStageStarted)
      {
        NextStage = stage;
        TransitionStart = DateTime.Now;
        SecondStageStarted = true;
      }
    }

    public void Update()
    {
      if (CurrentStage != NextStage)
        Transition();
      audioEngine.Update();

      if (LightVolume != TargetLightVolume)
        LightVolume += Math.Sign(TargetLightVolume - LightVolume) * 0.01f;
      if (DarkVolume != TargetDarkVolume)
        DarkVolume += Math.Sign(TargetDarkVolume - DarkVolume) * 0.01f;

      if (Math.Abs(LightVolume - TargetLightVolume) < 0.01f)
        LightVolume = TargetLightVolume;
      if (Math.Abs(DarkVolume - TargetDarkVolume) < 0.01f)
        DarkVolume = TargetDarkVolume;

      light.SetVolume(LightVolume);
      dark.SetVolume(DarkVolume);
    }

    private DateTime TransitionStart;
    private void Transition()
    {
      double milli = (DateTime.Now - TransitionStart).TotalMilliseconds;
      if (milli > 10000)
      {
        light2.SetVolume(1.0f);
        dark2.SetVolume(1.0f);
        CurrentStage = NextStage;
        return;
      }

      light2.SetVolume((float)milli / 10000);
      dark2.SetVolume((float)milli / 10000);
    }
  }
}
