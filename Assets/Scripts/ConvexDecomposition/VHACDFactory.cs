//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 3.0.12
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------

namespace ConvexDecomposition {

public class VHACDFactory {
  public static VHACD CreateVHACD() {
    global::System.IntPtr cPtr = VHACDBridge.CreateVHACD();
    VHACD ret = (cPtr == global::System.IntPtr.Zero) ? null : new VHACD(cPtr, false);
    return ret;
  }

  public static VHACD CreateVHACD_ASYNC() {
    global::System.IntPtr cPtr = VHACDBridge.CreateVHACD_ASYNC();
    VHACD ret = (cPtr == global::System.IntPtr.Zero) ? null : new VHACD(cPtr, false);
    return ret;
  }

  public static readonly int VHACD_VERSION_MAJOR = VHACDBridge.VHACD_VERSION_MAJOR_get();
  public static readonly int VHACD_VERSION_MINOR = VHACDBridge.VHACD_VERSION_MINOR_get();
}

}
