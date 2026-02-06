// using System;
// using Convai.Scripts.LoggerSystem;
// using Convai.Scripts.NotificationSystem;
// // using Grpc.Core;

// namespace Convai.Scripts.Services.ExceptionHandling
// {

//     public class ConvaiRPCExceptionHandling
//     {
//         private ConvaiUnityLogger _logger;
//         public ConvaiRPCExceptionHandling() => ConvaiUnityLogger.OnInitializationCompleted += OnInitializationCompleted;


//         ~ConvaiRPCExceptionHandling()
//         {
//             if (_logger == null)
//             {
//                 return;
//             }

//             _logger.RPCExceptionCaught -= RPCException;
//         }


//         private void OnInitializationCompleted(ConvaiUnityLogger logger) => logger.RPCExceptionCaught += RPCException;

//         private void RPCException(RpcException obj)
//         {
//             switch (obj.StatusCode)
//             {
//                 case StatusCode.OK:
//                     break;
//                 case StatusCode.Cancelled:
//                     break;
//                 case StatusCode.Unknown:
//                     break;
//                 case StatusCode.InvalidArgument:
//                     break;
//                 case StatusCode.DeadlineExceeded:
//                     break;
//                 case StatusCode.NotFound:
//                     break;
//                 case StatusCode.AlreadyExists:
//                     break;
//                 case StatusCode.PermissionDenied:
//                     ConvaiServices.NotificationService.RequestNotification(NotificationType.USAGE_LIMIT_EXCEEDED);
//                     break;
//                 case StatusCode.Unauthenticated:
//                     break;
//                 case StatusCode.ResourceExhausted:
//                     break;
//                 case StatusCode.FailedPrecondition:
//                     break;
//                 case StatusCode.Aborted:
//                     break;
//                 case StatusCode.OutOfRange:
//                     break;
//                 case StatusCode.Unimplemented:
//                     break;
//                 case StatusCode.Internal:
//                     break;
//                 case StatusCode.Unavailable:
//                     break;
//                 case StatusCode.DataLoss:
//                     break;
//                 default:
//                     throw new ArgumentOutOfRangeException();
//             }


//         }
//     }

// }


