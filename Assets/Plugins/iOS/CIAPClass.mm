//
//  CIAPClass.m
//
//
//#import "cocos2d.h"
//using namespace cocos2d;
#import "CIAPClass.h"
#import "AppDelegateListener.h"
#import "SBJson.h"
//#import "CangShuBundleAd.h"
//在内购项目中创的商品单号
#define ProductID_IAPp1 @"east2west.unWorded_1"

@interface UIViewController (CSCustom)
// 获取顶层VC
+ (UIViewController *)topVC;
@end

//-------------------------内购类---------------------------
@implementation CIAPClass
+(id)sharedManager
{
    static CIAPClass* sharedMyManager = nil;
    @synchronized(self) {
        
        if (sharedMyManager == nil)
            
            sharedMyManager = [[self alloc] init];
        
    }
    return sharedMyManager;
}
-(id) init
{
    if ((self = [super init])) {
        //----监听购买结果
        [[SKPaymentQueue defaultQueue] addTransactionObserver:self];
    }
    return self;
}

//发起恢复
-(void)restore
{
    [[SKPaymentQueue defaultQueue] restoreCompletedTransactions];
}

-(void)buy:(NSString*)type cost:(float)cost purchaseID:(NSString*)itemID purchaseDiscribe:(NSString*)dis
{
    buyType = type;
    nCost = cost;
    itemId=itemID;
    itemDes=dis;
    //isPurchasing=true;
    if ([SKPaymentQueue canMakePayments]) {
        [self RequestProductData];
        NSLog(@"允许程序内付费购买");
    }
    else
    {
        NSLog(@"不允许程序内付费购买");
//        UIAlertView *alerView =  [[UIAlertView alloc] initWithTitle:@"警告"
//                                                            message:@"You can‘t purchase in app store（不允许程序内付费购买）"
//                                                           delegate:nil cancelButtonTitle:NSLocalizedString(@"Close（关闭）",nil) otherButtonTitles:nil];
//
//        [alerView show];
        
        UIAlertController *alert = [UIAlertController alertControllerWithTitle:@"警告"
                                                                       message:@"You can‘t purchase in app store（不允许程序内付费购买）"
                                                                preferredStyle:  UIAlertControllerStyleAlert];
        [alert addAction:[UIAlertAction actionWithTitle:@"Close（关闭）"
                                                  style:UIAlertActionStyleDefault
                                                handler:^(UIAlertAction * _Nonnull action) {/*点击按钮的响应事件；*/}]];
        //弹出提示框；
        [self presentViewController:alert animated:true completion:nil];
        
        
    }
}

-(bool)CanMakePay
{
    return [SKPaymentQueue canMakePayments];
}

-(void)RequestProductData
{
    NSLog(@"---------请求对应的产品信息------------");
    NSSet *set = [NSSet setWithObjects:buyType, nil];
    SKProductsRequest *request=[[SKProductsRequest alloc] initWithProductIdentifiers: set];
    request.delegate=self;
    [request start];
    
}
//<SKProductsRequestDelegate> 请求协议
//收到的产品信息
- (void)productsRequest:(SKProductsRequest *)request didReceiveResponse:(SKProductsResponse *)response{
    
    //    NSLog(@"-----------收到产品反馈信息--------------");
    //    NSArray *myProduct = response.products;
    //    NSLog(@"产品Product ID:%@",response.invalidProductIdentifiers);
    //    NSLog(@"产品付费数量: %d", [myProduct count]);
    //    // populate UI
    //    for(SKProduct *product in myProduct){ß
    //        NSLog(@"product info");
    //        NSLog(@"SKProduct 描述信息%@", [product description]);
    //        NSLog(@"产品标题 %@" , product.localizedTitle);
    //        NSLog(@"产品描述信息: %@" , product.localizedDescription);
    //        NSLog(@"价格: %@" , product.price);
    //        NSLog(@"Product id: %@" , product.productIdentifier);
    //    }
    
//    SKPayment *payment = [SKPayment paymentWithProductIdentifier:buyType];
//    NSLog(@"---------发送购买请求------------");
//    [[SKPaymentQueue defaultQueue] addPayment:payment];
    
    NSLog(@"--------------收到产品反馈消息---------------------");
    NSArray *product = response.products;
    if([product count] == 0){
        NSLog(@"--------------没有商品------------------");
        return;
    }
    
    NSLog(@"productID:%@", response.invalidProductIdentifiers);
    NSLog(@"产品付费数量:%lu",(unsigned long)[product count]);
    
    SKProduct *p = nil;
    for (SKProduct *pro in product) {
        NSLog(@"%@", [pro description]);
        NSLog(@"%@", [pro localizedTitle]);
        NSLog(@"%@", [pro localizedDescription]);
        NSLog(@"%@", [pro price]);
        NSLog(@"%@", [pro productIdentifier]);
        
        if([pro.productIdentifier isEqualToString:buyType]){
            p = pro;
        }
    }
    
    SKPayment *payment = [SKPayment paymentWithProduct:p];
    
    NSLog(@"发送购买请求");
    [[SKPaymentQueue defaultQueue] addPayment:payment];
    
}
- (void)requestProUpgradeProductData
{
    NSLog(@"------请求升级数据---------");
    NSSet *productIdentifiers = [NSSet setWithObject:@"com.productid"];
    SKProductsRequest* productsRequest = [[SKProductsRequest alloc] initWithProductIdentifiers:productIdentifiers];
    productsRequest.delegate = self;
    [productsRequest start];
    
}
//弹出错误信息
- (void)request:(SKRequest *)request didFailWithError:(NSError *)error{
    NSLog(@"-------弹出错误信息----------");
//    UIAlertView *alerView =  [[UIAlertView alloc] initWithTitle:NSLocalizedString(@"警告",NULL) message:[error localizedDescription]
//                                                       delegate:nil cancelButtonTitle:NSLocalizedString(@"Close",nil) otherButtonTitles:nil];
//    [alerView show];
    
}

-(void) requestDidFinish:(SKRequest *)request
{
    NSLog(@"----------反馈信息结束--------------");
    
}

-(void) PurchasedTransaction: (SKPaymentTransaction *)transaction{
    NSLog(@"-----PurchasedTransaction----");
    NSArray *transactions =[[NSArray alloc] initWithObjects:transaction, nil];
    [self paymentQueue:[SKPaymentQueue defaultQueue] updatedTransactions:transactions];
    
}

//<SKPaymentTransactionObserver> 千万不要忘记绑定，代码如下：
//----监听购买结果
- (void)paymentQueue:(SKPaymentQueue *)queue updatedTransactions:(NSArray *)transactions//交易结果
{
    NSLog(@"-----paymentQueue--------");
    for (SKPaymentTransaction *transaction in transactions)
    {
        switch (transaction.transactionState)
        {
            case SKPaymentTransactionStatePurchased://交易完成
            {

               if([self isBlankString:itemId])
               {
                   NSLog(@"-----isPurchasing err--------");
                   return;
               }
               NSLog(@"-----交易完成 --------%@",buyType);

                NSData*receipt= nil;
                NSString *version = [UIDevice currentDevice].systemVersion;
//                if (version.doubleValue >= 7.0) {
//                    // 针对 7.0 以上的iOS系统进行处理
////                    NSURL *receiptURL = [[NSBundle mainBundle] appStoreReceiptURL];
////                    receipt = [NSData dataWithContentsOfURL:receiptURL];
//
//                    NSURLRequest *urlRequest = [NSURLRequest requestWithURL:[[NSBundle mainBundle] appStoreReceiptURL]];
//                    NSError *error = nil;
//                    receipt = [NSURLConnection sendSynchronousRequest:urlRequest returningResponse:nil error:&error];
//
////                    NSString *receiptURLStr = [receiptURL absoluteString];
////                    NSRange rangeSandbox = [receiptURLStr rangeOfString:@"sandbox"];
////                    if (rangeSandbox.location != NSNotFound){
////                        record[kIAPEnvironment] = [NSNumber numberWithInt:1];
////                    }
//                } else
                {
                    //ios 3.0~7.0
                    receipt = transaction.transactionReceipt;
                }
                
                //NSString* aStr= [[NSString alloc] initWithData:receipt   encoding:NSUTF8StringEncoding];
                //NSString* aStr1=  [self encode:(uint8_t *)transaction.transactionReceipt.bytes
                //                      length:transaction.transactionReceipt.length];
				  
                NSString* aStr1=[self encode:(uint8_t *)transaction.transactionReceipt.bytes length:transaction.transactionReceipt.length];
				  
                //NSLog(@"-----transactionState str--------\r\n %@",aStr);
                //NSLog(@"-----transactionState str--------\r\n %@",aStr1);
                NSLog(@"-----transactionState str--------\r\n %@",[self getPaymentStateStr:transaction.transactionState]);
                
                NSString *nsStr=[NSString stringWithFormat:@"%@,%@,%@,%@,%@,%@",
                                 itemId, transaction.transactionIdentifier,
                                 aStr1,@"1",
                                 [self getPaymentStateStr:transaction.transactionState],@"downloads" ];
                //or=red>{"1":{"productIdentifier":"productIdentifier11","transactionIdentifier":"transactionIdentifier11","base64EncodedReceipt":"base64EncodedReceipt11","quantity":"quantity11","transactionState":"transactionState11","downloads":"downloads11"}}</color>
                //保存数据
                [self setTransitionReceipt:transaction.payment.productIdentifier
                                   receipt:aStr1
                             transactionId:transaction.transactionIdentifier];
                UnitySendMessage("Menu_Shop","PaySuccess",[nsStr UTF8String]);
                itemId=@"";
                //NSLog(@"-----Purchasing str--------\r\n %@",nsStr);
                
                [self completeTransaction:transaction];
                UIAlertController *alert = [UIAlertController alertControllerWithTitle:@"提示"
                                                                               message:@"购买成功！"
                                                                        preferredStyle:  UIAlertControllerStyleAlert];
                [alert addAction:[UIAlertAction actionWithTitle:@"Close（关闭）"
                                                          style:UIAlertActionStyleDefault
                                                        handler:^(UIAlertAction * _Nonnull action) {/*点击按钮的响应事件；*/}]];
                //弹出提示框；
                [self presentViewController:alert animated:true completion:nil];
            }
            break;
            case SKPaymentTransactionStateFailed://交易失败
            {
                [self failedTransaction:transaction];
                NSLog(@"-----交易失败 --------");
                NSString *temp=@"...";
                UnitySendMessage("Menu_Shop","PayFailed",[temp UTF8String]);
                UIAlertController *alert = [UIAlertController alertControllerWithTitle:@"提示"
                                                                               message:@"购买失败，请重新尝试购买！"
                                                                        preferredStyle:  UIAlertControllerStyleAlert];
                [alert addAction:[UIAlertAction actionWithTitle:@"Close（关闭）"
                                                          style:UIAlertActionStyleDefault
                                                        handler:^(UIAlertAction * _Nonnull action) {/*点击按钮的响应事件；*/}]];
                //弹出提示框；
                [self presentViewController:alert animated:true completion:nil];
                itemId=@"";
            }
                break;
            case SKPaymentTransactionStateRestored://已经购买过该商品
            {
                [self restoreTransaction:transaction];
                NSLog(@"-----已经购买过该商品 --------");
            }
                break;
            case SKPaymentTransactionStatePurchasing:      //商品添加进列表
            {
                NSLog(@"-----商品添加进列表 --------");
            }
                break;
            default:
            {
            }
                break;
        }
    }
}
- (NSString *)encode:(const uint8_t *)input length:(NSInteger)length {
    static char table[] = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
    
    NSMutableData *data = [NSMutableData dataWithLength:((length + 2) / 3) * 4];
    uint8_t *output = (uint8_t *)data.mutableBytes;
    
    for (NSInteger i = 0; i < length; i += 3) {
        NSInteger value = 0;
        for (NSInteger j = i; j < (i + 3); j++) {
            value <<= 8;
            
            if (j < length) {
                value |= (0xFF & input[j]);
            }
        }
        
        NSInteger index = (i / 3) * 4;
        output[index + 0] =                    table[(value >> 18) & 0x3F];
        output[index + 1] =                    table[(value >> 12) & 0x3F];
        output[index + 2] = (i + 1) < length ? table[(value >> 6)  & 0x3F] : '=';
        output[index + 3] = (i + 2) < length ? table[(value >> 0)  & 0x3F] : '=';
    }
    
    return [[NSString alloc] initWithData:data encoding:NSASCIIStringEncoding];
}
-(NSString*) getPaymentStateStr:(SKPaymentTransactionState)payment;{
    switch (payment) {
        case SKPaymentTransactionStatePurchasing:
            return @"Purchasing";
            break;
        case SKPaymentTransactionStatePurchased:
            return @"Purchased";
            break;
        case SKPaymentTransactionStateFailed:
            return @"Failed";
            break;
        case SKPaymentTransactionStateRestored:
            return @"Restored";
            break;
        case SKPaymentTransactionStateDeferred:
            return @"Deferred";
            break;
        default:
            break;
    }
    return @"err";
}
// - (NSString *)encode:(const uint8_t *)input length:(NSInteger)length {
    // static char table[] = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
    
    // NSMutableData *data = [NSMutableData dataWithLength:((length + 2) / 3) * 4];
    // uint8_t *output = (uint8_t *)data.mutableBytes;
    
    // for (NSInteger i = 0; i < length; i += 3) {
        // NSInteger value = 0;
        // for (NSInteger j = i; j < (i + 3); j++) {
            // value <<= 8;
            
            // if (j < length) {
                // value |= (0xFF & input[j]);
            // }
        // }
        
        // NSInteger index = (i / 3) * 4;
        // output[index + 0] =                    table[(value >> 18) & 0x3F];
        // output[index + 1] =                    table[(value >> 12) & 0x3F];
        // output[index + 2] = (i + 1) < length ? table[(value >> 6)  & 0x3F] : '=';
        // output[index + 3] = (i + 2) < length ? table[(value >> 0)  & 0x3F] : '=';
    // }
    
    // return [[[NSString alloc] initWithData:data encoding:NSASCIIStringEncoding] autorelease];
// }
//typedef NS_ENUM(NSInteger, SKPaymentTransactionState) {
//    SKPaymentTransactionStatePurchasing,    // Transaction is being added to the server queue.
//    SKPaymentTransactionStatePurchased,     // Transaction is in queue, user has been charged.  Client should complete the transaction.
//    SKPaymentTransactionStateFailed,        // Transaction was cancelled or failed before being added to the server queue.
//    SKPaymentTransactionStateRestored,      // Transaction was restored from user's purchase history.  Client should complete the transaction.
//    SKPaymentTransactionStateDeferred NS_ENUM_AVAILABLE_IOS(8_0),   // The transaction is in the queue, but its final status is pending external action.
//} NS_AVAILABLE_IOS(3_0);

- (BOOL)isBlankString:(NSString *)aStr {
    if (!aStr) {
        return YES;
    }
    if ([aStr isKindOfClass:[NSNull class]]) {
        return YES;
    }
    if (!aStr.length) {
        return YES;
    }
    NSCharacterSet *set = [NSCharacterSet whitespaceAndNewlineCharacterSet];
    NSString *trimmedStr = [aStr stringByTrimmingCharactersInSet:set];
    if (!trimmedStr.length) {
        return YES;
    }
    return NO;
}
- (void) completeTransaction: (SKPaymentTransaction *)transaction

{
    NSLog(@"-----交易结束--------");
    // Your application should implement these two methods.
    NSString *product = transaction.payment.productIdentifier;
    if ([product length] > 0) {
        
        NSArray *tt = [product componentsSeparatedByString:@"."];
        NSString *bookid = [tt lastObject];
        if ([bookid length] > 0) {
            [self recordTransaction:bookid];
            [self provideContent:bookid];
        }
    }
    
    // Remove the transaction from the payment queue.
    
    [[SKPaymentQueue defaultQueue] finishTransaction: transaction];
    
}

//记录交易
-(void)recordTransaction:(NSString *)product{
    NSLog(@"-----记录交易--------");
}

//处理下载内容
-(void)provideContent:(NSString *)product{
    NSLog(@"-----下载--------");
}

- (void) failedTransaction: (SKPaymentTransaction *)transaction{
    NSLog(@"失败");
    if (transaction.error.code != SKErrorPaymentCancelled)
    {
    }
    [[SKPaymentQueue defaultQueue] finishTransaction: transaction];
    
    
}
-(void) paymentQueueRestoreCompletedTransactionsFinished: (SKPaymentTransaction *)transaction{
    NSLog(@"paymentQueueRestoreCompletedTransactionsFinished called:");
    NSLog(@"SKPaymentQueue:%@",transaction);
    NSLog(@"=======================================================");
}

- (void) restoreTransaction: (SKPaymentTransaction *)transaction
{
    NSLog(@" 交易恢复处理");
    //    LOG("IOSGameCenter::paymentQueue(): %s state=%d\n", [transaction.payment.productIdentifier UTF8String], transaction.transactionState);
    //    ChannelDef::IOSBuyStore([transaction.payment.productIdentifier UTF8String]);
    //恢复不可重复购买的道具
    //UnitySendMessage("Purchaser","ProcessPurchase",[transaction.payment.productIdentifier UTF8String]);
    UnitySendMessage("DontDestroy_Qin","PaySeccess",[buyType UTF8String]);
}

-(void) paymentQueue:(SKPaymentQueue *) paymentQueue restoreCompletedTransactionsFailedWithError:(NSError *)error{
    NSLog(@"restoreCompletedTransactionsFailedWithError called:");
    NSLog(@"error:%@",error);
    NSLog(@"=======================================================");
}


#pragma mark connection delegate
- (void)connection:(NSURLConnection *)connection didReceiveData:(NSData *)data
{
    //   NSLog(@"%@",  [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding]);
}
- (void)connectionDidFinishLoading:(NSURLConnection *)connection{
    
}

- (void)connection:(NSURLConnection *)connection didReceiveResponse:(NSURLResponse *)response{
    switch([(NSHTTPURLResponse *)response statusCode]) {
        case 200:
        case 206:
            break;
        case 304:
            break;
        case 400:
            break;
        case 404:
            break;
        case 416:
            break;
        case 403:
            break;
        case 401:
        case 500:
            break;
        default:
            break;
    }
}

- (void)connection:(NSURLConnection *)connection didFailWithError:(NSError *)error {
    NSLog(@"test");
}

-(void)dealloc
{
    [[SKPaymentQueue defaultQueue] removeTransactionObserver:self];//解除监听
}

//----------------------------------------广告类------------------------------------------
//#import "GDTMobBannerView.h" //导入GDTMobBannerView.h头文件
//@interface BannerViewController :
//UIViewController<GDTMobBannerViewDelegate>
//{
//GDTMobBannerView *_bannerView;//声明一个GDTMobBannerView的实例
//}

NSString * paymentID;
NSString * paymentReceipt;
NSString * paymentTransactionID;

-(void)setTransitionReceipt:(NSString *)payload receipt:(NSString *)receipt transactionId:(NSString *)transactionId
{
    paymentID=payload;
    paymentReceipt=receipt;
    paymentTransactionID=transactionId;
    
    SBJsonWriter *writer = [[SBJsonWriter alloc] init];
    NSString * deviceUUID = [[[UIDevice currentDevice] identifierForVendor] UUIDString];
    NSDictionary *jsonDictionary= [NSDictionary dictionaryWithObjectsAndKeys:
                                   paymentID,@"paymentID",
                                   paymentReceipt,@"receipt",
                                   paymentTransactionID,@"orderID",
                                   deviceUUID,@"deviceID",nil];
    NSString *jasonString = [writer stringWithObject:jsonDictionary];
    
    NSLog(@"test%@",jasonString);
    
    UnitySendMessage("PurchaseManager","getReceiptSucess",[jasonString UTF8String]);
}

@end
