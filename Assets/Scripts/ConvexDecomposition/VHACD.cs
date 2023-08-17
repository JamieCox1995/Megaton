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

public class VHACD : IVHACD, global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal VHACD(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(VHACD obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          throw new global::System.MethodAccessException("C++ destructor does not have public access");
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  public virtual void Cancel() {
    VHACDBridge.VHACD_Cancel(swigCPtr);
  }

  public virtual bool Compute(float[] points, uint stridePoints, uint countPoints, int[] triangles, uint strideTriangles, uint countTriangles, Parameters arg6) {
    bool ret = VHACDBridge.VHACD_Compute__SWIG_0(swigCPtr, points, stridePoints, countPoints, triangles, strideTriangles, countTriangles, Parameters.getCPtr(arg6));
    if (VHACDBridge.SWIGPendingException.Pending) throw VHACDBridge.SWIGPendingException.Retrieve();
    return ret;
  }

  public virtual bool Compute(double[] points, uint stridePoints, uint countPoints, int[] triangles, uint strideTriangles, uint countTriangles, Parameters arg6) {
    bool ret = VHACDBridge.VHACD_Compute__SWIG_1(swigCPtr, points, stridePoints, countPoints, triangles, strideTriangles, countTriangles, Parameters.getCPtr(arg6));
    if (VHACDBridge.SWIGPendingException.Pending) throw VHACDBridge.SWIGPendingException.Retrieve();
    return ret;
  }

  public virtual uint GetNConvexHulls() {
    uint ret = VHACDBridge.VHACD_GetNConvexHulls(swigCPtr);
    return ret;
  }

  public virtual void GetConvexHull(uint index, ConvexHull ch) {
    VHACDBridge.VHACD_GetConvexHull(swigCPtr, index, ConvexHull.getCPtr(ch));
    if (VHACDBridge.SWIGPendingException.Pending) throw VHACDBridge.SWIGPendingException.Retrieve();
  }

  public virtual void Clean() {
    VHACDBridge.VHACD_Clean(swigCPtr);
  }

  public virtual void Release() {
    VHACDBridge.VHACD_Release(swigCPtr);
  }

  public virtual bool OCLInit(global::System.IntPtr oclDevice, UserLogger logger) {
    bool ret = VHACDBridge.VHACD_OCLInit__SWIG_0(swigCPtr, oclDevice, UserLogger.getCPtr(logger));
    return ret;
  }

  public virtual bool OCLInit(global::System.IntPtr oclDevice) {
    bool ret = VHACDBridge.VHACD_OCLInit__SWIG_1(swigCPtr, oclDevice);
    return ret;
  }

  public virtual bool OCLRelease(UserLogger logger) {
    bool ret = VHACDBridge.VHACD_OCLRelease__SWIG_0(swigCPtr, UserLogger.getCPtr(logger));
    return ret;
  }

  public virtual bool OCLRelease() {
    bool ret = VHACDBridge.VHACD_OCLRelease__SWIG_1(swigCPtr);
    return ret;
  }

  public virtual bool IsReady() {
    bool ret = VHACDBridge.VHACD_IsReady(swigCPtr);
    return ret;
  }

}

}
