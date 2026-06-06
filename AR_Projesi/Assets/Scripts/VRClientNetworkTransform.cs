// VRClientNetworkTransform.cs
// İstemci yetkili NetworkTransform. Sahibi olan istemci konumu yazar,
// sunucu sadece tüm diğer istemcilere ileterek aktarır.
// VR'da elle tutulan nesneler için kritik: hareket gecikmesi minimize edilir.

using Unity.Netcode.Components;

namespace AREgitim.VR
{
    /// <summary>
    /// VR el etkileşimi için istemci yetkili (owner-authoritative) NetworkTransform.
    /// NGO 1.x: OnIsServerAuthoritative override ile değiştirilir.
    /// </summary>
    public class VRClientNetworkTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative() => false;
    }
}
