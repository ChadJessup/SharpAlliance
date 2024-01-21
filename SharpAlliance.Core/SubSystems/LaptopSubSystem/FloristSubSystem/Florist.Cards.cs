using System;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem.FloristSubSystem;

public partial class Florist
{
    private static bool[] FloristGallerySubPagesVisitedFlag = new bool[4];

    public static void GameInitFloristCards()
    {

    }

    internal static void EnterFlorist()
    {
        throw new NotImplementedException();
    }

    internal static void EnterFloristCards()
    {
        throw new NotImplementedException();
    }

    internal static void EnterFloristGallery()
    {
        throw new NotImplementedException();
    }

    internal static void EnterFloristOrderForm()
    {
        throw new NotImplementedException();
    }

    internal static void EnterInitFloristGallery()
    {
        Array.Fill(FloristGallerySubPagesVisitedFlag, false);
    }

    internal static void ExitFlorist()
    {
        throw new NotImplementedException();
    }

    internal static void ExitFloristCards()
    {
        throw new NotImplementedException();
    }

    internal static void ExitFloristGallery()
    {
        throw new NotImplementedException();
    }

    internal static void ExitFloristOrderForm()
    {
        throw new NotImplementedException();
    }

    internal static void HandleFlorist()
    {
        throw new NotImplementedException();
    }

    internal static void HandleFloristCards()
    {
        throw new NotImplementedException();
    }

    internal static void HandleFloristGallery()
    {
        throw new NotImplementedException();
    }

    internal static void HandleFloristOrderForm()
    {
        throw new NotImplementedException();
    }
}
