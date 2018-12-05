if (!IsMac)
  return;

AppleCodesignIdentity(Env("APPLECODESIGNIDENTITY"),Env("APPLECODESIGNIDENTITYURL"));
AppleCodesignProfile(Env("APPLECODESIGNPROFILEURL"));
