if (!IsMac)
  return;

Log.Information (Env("APPLECODESIGNIDENTITYURL"));
Log.Information (Env("APPLECODESIGNPROFILEURL"));
AppleCodesignIdentity("iPhone Developer: Xamarin QA (JP4JS5NR3R)",Env("APPLECODESIGNIDENTITYURL"));
AppleCodesignProfile(Env("APPLECODESIGNPROFILEURL"));
