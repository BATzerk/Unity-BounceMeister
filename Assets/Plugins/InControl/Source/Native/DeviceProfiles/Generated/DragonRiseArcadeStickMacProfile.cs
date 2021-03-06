namespace InControl.NativeProfile
{
	using System;


	// @cond nodoc
	public class DragonRiseArcadeStickMacProfile : Xbox360DriverMacProfile
	{
		public DragonRiseArcadeStickMacProfile()
		{
			Name = "DragonRise Arcade Stick";
			Meta = "DragonRise Arcade Stick on Mac";

			Matchers = new[] {
				new NativeInputDeviceMatcher {
					VendorID = 0x0079,
					ProductID = 0x187c,
				},
			};
		}
	}
	// @endcond
}


