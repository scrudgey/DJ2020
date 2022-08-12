using System.Collections.Generic;
using System.Linq;
// public struct SuspicionData {
//     public Suspiciousness interactorSuspicion;
//     public Suspiciousness audioSuspicion;
//     public Suspiciousness appearanceSuspicion;
//     public Suspiciousness itemHandlerSuspicion;
//     public SensitivityLevel levelSensitivity;

//     public Suspiciousness playerActivity() {
//         return Toolbox.Max<Suspiciousness>(
//             interactorSuspicion,
//             itemHandlerSuspicion
//         );
//     }

//     public Suspiciousness netValue() {
//         return new List<Suspiciousness>{
//             interactorSuspicion,
//             itemHandlerSuspicion,
//             audioSuspicion,
//             appearanceSuspicion
//         }.Aggregate(Suspiciousness.normal, Toolbox.Max<Suspiciousness>);
//     }
// }
