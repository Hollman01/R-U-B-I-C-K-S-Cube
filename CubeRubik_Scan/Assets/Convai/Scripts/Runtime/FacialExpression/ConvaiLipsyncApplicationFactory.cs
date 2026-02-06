// using Convai.Scripts.Facial_Expression.Types;
// using Service;

// namespace Convai.Scripts.Facial_Expression
// {
//     public static class ConvaiLipsyncApplicationFactory
//     {
//         public static ConvaiLipsyncApplicationBase CreateLipsyncApplicationBase(ConvaiNPC convaiNPC,ConvaiFacialExpressionController lipsyncController)
//         {
//             switch (convaiNPC.FaceModel)
//             {
//                 case FaceModel.OvrModelName:
//                     return new ConvaiVisemeLipsync(convaiNPC, lipsyncController);
//                 default:
//                     return new ConvaiEmptyLipsync(convaiNPC, lipsyncController);
//             }
//         }
//     }
// }


