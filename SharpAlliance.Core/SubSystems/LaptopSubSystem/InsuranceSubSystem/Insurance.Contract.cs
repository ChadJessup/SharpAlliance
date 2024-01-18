namespace SharpAlliance.Core.SubSystems.LaptopSubSystem.InsuranceSubSystem;

public partial class Insurance
{
    private static int gsCurrentInsuranceMercIndex;

    public static void GameInitInsuranceContract()
    {
        gsCurrentInsuranceMercIndex = gTacticalStatus.Team[gbPlayerNum].bFirstID;
    }
}
