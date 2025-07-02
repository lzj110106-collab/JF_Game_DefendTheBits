//
//  Tfy_Interface.m
//  Unity-iPhone
//
//  Created by iMac on 2017/9/28.
//
//

#import <Foundation/Foundation.h>
#import <string.h>
//#import <TFYLYSDK/TFYLYSDK.h>
#import "Tfy_Interface.h"
#import "CIAPClass.h"
#import <GameKit/GameKit.h>


void InitSdk()
{
}

void Login()
{
    //    //sdk
    [[Tfy_Interface Tfy_local] InitSDKIn];
    //    //login支持重复调用，每次收到登出消息，执行完游戏登出代码后，记得调用一次login接口
    //    //[[TFYSDKManager sharedManager] Login];
    //    NSLog(@"Login");
    //    //[[TFYSDKManager sharedManager] showBindView];
    //
    //    UnitySendMessage("DontDestroy_Qin","SetTfyToken","cammer_05");
    //    UnitySendMessage("DontDestroy_Qin","LoginSeccess","158296");
}

void ExchangeUser()
{
    NSLog(@"exchangeuser");
    //[[TFYSDKManager sharedManager] changeAccount];
    
}

void Purchase(float price,const char* a,const char* b,const char* c,const char* d){
    NSLog(@"price:%f",price);
    NSLog(@"purchaseID:%s",a);
    NSLog(@"purchaseItem:%s",b);
    NSLog(@"purchaseDiscribe:%s",c);
    //price=1.0f;
    [[Tfy_Interface Tfy_local] PurchaseItem:price purchaseID:a purchaseItem:b purchaseDiscribe:c userID:d];
}
void OnUpload()
{
    NSLog(@"OnUpload");
    //发货成功后，调用getConfirmOrder方法，sdk确认已发货。
    //发货失败，无需调用
    //[[TFYSDKManager sharedManager] getConfirmOrder:_payDic[@"orderid"]];
}

@interface Tfy_Interface ()

@property (nonatomic, strong) NSDictionary *userDic;
@property (nonatomic, strong) NSDictionary *payDic;

@end
@implementation Tfy_Interface

static NSString *userName=@"";
static NSString *userToken=@"";
static NSString *orderID=@"";
static NSString *PurductID=@"";
static NSString *is_bind_moblie=false;
static bool isInitsdk=false;

static NSString* buyType;
static NSString* buyTypeReal;
int nCost;

+(id)Tfy_local {
    
    static Tfy_Interface *center = nil;
    if (center == nil) {
        center = [[Tfy_Interface alloc] init];
    }
    return center;
}

-(void)InitSDKIn
{
    NSLog(@"InitSdk");
    //    if(!isInitsdk)
    //    {
    //        //[[TFYSDKManager sharedManager] SDKInitWithAgent:@"app-12-01" Server:nil];
    //        //[[TFYSDKManager sharedManager] SDKInitWithAgent:@"app-12-01" Server:nil];
    //        //设置回调
    //        [[Tfy_Interface Tfy_local] InitCallBack];
    //        isInitsdk=true;
    //
    //        GKLocalPlayer *localPlayer=GKLocalPlayer.localPlayer;
    //        localPlayer.authenticateHandler = ^(UIViewController *viewController, NSError *error)
    //        {
    //            if(viewController != nil)
    //            {
    //                NSLog(@"[SocialGamingImpl::authenticate] Player need to be loged in Game Center\n");
    //
    //            }
    //            else if ([GKLocalPlayer localPlayer].isAuthenticated)
    //            {
    //                if(error != nil)
    //                {
    //                    // Exception
    //                    return; //some sort of error, can't authenticate right now
    //                }
    //                userName = [GKLocalPlayer localPlayer].playerID;
    //            }
    //            else
    //            {
    //                NSLog(@"[SocialGamingImpl::authenticate] Player is not authenticated in Game Center - disable Game Center (blockLocalPlayer %p)", [GKLocalPlayer localPlayer]);
    //
    //            }
    //
    //        };
    //    }
    //    if(userName!=nil&&userName.length>0)
    //    {
    //        UnitySendMessage("DontDestroy_Qin","SetTfyToken","cammer_05");
    //        UnitySendMessage("DontDestroy_Qin","LoginSeccess",[userName UTF8String]);
    //    }
    //    else
    //    {
    //
    //        typedef NS_ENUM(NSInteger, UIAlertActionStyle) {
    //            UIAlertActionStyleDefault = 0,    //默认
    //            UIAlertActionStyleCancel,    //在左边   不能同时设置2个）
    //            UIAlertActionStyleDestructive   //变为红色
    //        } NS_ENUM_AVAILABLE_IOS(8_0);
    //        UIAlertController *alert = [UIAlertController alertControllerWithTitle:@"提示"
    //                                                                       message:@"未打开GameCenter功能！请在[设置->GameCenter]中开启后重试！"
    //                                                                preferredStyle:  UIAlertControllerStyleAlert];
    //        //确定
    //        UIAlertAction *okAlert = [UIAlertAction actionWithTitle:@"确定" style:UIAlertActionStyleDefault handler:^(UIAlertAction *action){
    //
    //        }];
    //        //取消
    //        UIAlertAction *cancelAlert = [UIAlertAction actionWithTitle:@"取消" style:UIAlertActionStyleDestructive handler:^(UIAlertAction *action){
    //            //具体操作内容
    //
    //        }];
    //
    //        [alert addAction:okAlert];
    //        [alert addAction:cancelAlert];
    //        UIViewController *top = [UIApplication sharedApplication].keyWindow.rootViewController;
    //        //弹出提示框；
    //        [top presentViewController:alert animated:true completion:nil];
    //
    //        //        typedef NS_ENUM(NSInteger, UIAlertActionStyle) {
    //        //            UIAlertActionStyleDefault = 0,    //默认
    //        //            UIAlertActionStyleCancel,    //在左边   不能同时设置2个）
    //        //            UIAlertActionStyleDestructive   //变为红色
    //        //        } NS_ENUM_AVAILABLE_IOS(8_0);
    //        //        UIAlertController *alert = [UIAlertController alertControllerWithTitle:@"你确定退出?" message:nil preferredStyle:UIAlertControllerStyleAlert];
    //        //        //UIAlertActionStyleDefault
    //        //        //UIAlertActionStyleCancel     //在左边   （不能同时设置2个）
    //        //        //UIAlertActionStyleDestructive  //变为红色
    //        //        //确定
    //        //        UIAlertAction *okAlert = [UIAlertAction actionWithTitle:@"确定" style:UIAlertActionStyleDefault handler:^(UIAlertAction *action){
    //        //            //具体操作内容
    //        //
    //        //        }];
    //        //        //取消
    //        //        UIAlertAction *cancelAlert = [UIAlertAction actionWithTitle:@"取消" style:UIAlertActionStyleDestructive handler:^(UIAlertAction *action){
    //        //            //具体操作内容
    //        //
    //        //        }];
    //        //        [alert addAction:okAlert];
    //        //        [alert addAction:cancelAlert];
    //        //        UIViewController *top = [UIApplication sharedApplication].keyWindow.rootViewController;
    //        //        [top presentViewController:alert animated:YES completion:nil];
    //    }
    UnitySendMessage("DontDestroy_Qin","SetTfyToken","cammer_05");
    UnitySendMessage("DontDestroy_Qin","LoginSeccess","anything");
}

- (void)InitCallBack {
    [[NSNotificationCenter defaultCenter] addObserver: self
                                             selector: @selector(handleNote:)
                                                 name: @"TFYSDK_LOGIN_SUCCESS"
                                               object: nil];
    [[NSNotificationCenter defaultCenter] addObserver: self
                                             selector: @selector(handleNote:)
                                                 name: @"TFYSDK_LOGOUT_BEGIN"
                                               object: nil];
    [[NSNotificationCenter defaultCenter] addObserver: self
                                             selector: @selector(handleNote:)
                                                 name: @"TFYSDK_PAY_SUCCESS"
                                               object: nil];
}
- (void)handleNote:(NSNotification *)notification{
    if ([[notification name] isEqualToString:@"TFYSDK_LOGIN_SUCCESS"]){
        
        //我方返回userid，token，is_bind_mobile。cp请接收。其中，token可用于后端验证。userid必须接收，和判断。得到userid，方可进入游戏。
        
        _userDic = [notification userInfo];
        userToken=_userDic[@"token"];
        userName=_userDic[@"userid"];
        is_bind_moblie=_userDic[@"is_bind_moblie"];
        UnitySendMessage("DontDestroy_Qin","SetTfyToken",[userToken UTF8String]);
        UnitySendMessage("DontDestroy_Qin","LoginSeccess",[userName UTF8String]);
        //值1或者0或者-1
        //     1：该username绑定了手机
        //     0：该username没有绑定手机
        //   －1： 不知道是否绑定
        
    }
    else if ([[notification name] isEqualToString:@"TFYSDK_LOGOUT_BEGIN"]){
        //此处为cp游戏登出代码。务必填写。sdk发送游戏登出消息。
        
        //[[TFYSDKManager sharedManager] Login];
    }
    else if ([[notification name] isEqualToString:@"TFYSDK_PAY_SUCCESS"]){
        //支付成功，开始发货
        _payDic = [notification userInfo];//包含订单号，金额等
        orderID=_payDic[@"orderid"];
        PurductID=_payDic[@"productid"];
        
        //游戏发货代码。。。
        UnitySendMessage("DontDestroy_Qin","BackUpOrderid",[orderID UTF8String]);
        UnitySendMessage("Menu_Shop","PaySuccess",[PurductID UTF8String]);
        //发货成功后，调用getConfirmOrder方法，sdk确认已发货。
        //发货失败，无需调用
        //[[TFYSDKManager sharedManager] getConfirmOrder:_payDic[@"orderid"]];
    }
}
-(void)PurchaseItem:(float)price
         purchaseID:(const char *)a
       purchaseItem:(const char *)b
   purchaseDiscribe:(const char *)c
             userID:(const char *)d
{
    NSDictionary *purcDic = @{@"agent":@"app-12-01",
                              @"userid":userName,
                              @"H5ProductionDes":[NSString stringWithFormat:@"%s",c],
                              @"H5ProductionPrice":[NSString stringWithFormat:@"%f",price],
                              @"productid":[NSString stringWithFormat:@"%s",a],
                              @"gamename":@"wow",
                              @"roleid":[NSString stringWithFormat:@"%s",d],
                              @"serverid":@"34",
                              @"attach":@"123456abc"};
    //[[TFYSDKManager sharedManager] payWithPayDic:purcDic];
    buyType = [NSString stringWithUTF8String:a];
    nCost = price;
    
    
    //    if ([SKPaymentQueue canMakePayments]) {
    //        [self RequestProductData];
    //        NSLog(@"允许程序内付费购买");
    //    }
    //    else
    //    {
    //        NSLog(@"不允许程序内付费购买");
    //        /*
    //        UIAlertView *alerView =  [[UIAlertView alloc] initWithTitle:@"警告"
    //                                                            message:@"You can‘t purchase in app store（不允许程序内付费购买）"
    //                                                           delegate:nil
    //                                                  cancelButtonTitle:NSLocalizedString(@"Close（关闭）",nil) otherButtonTitles:nil];
    //        [alerView show];
    //        */
    //
    //        UIAlertController *alert = [UIAlertController alertControllerWithTitle:@"警告"
    //                                                                message:@"You can‘t purchase in app store（不允许程序内付费购买）"
    //                                                                preferredStyle:  UIAlertControllerStyleAlert];
    //        [alert addAction:[UIAlertAction actionWithTitle:@"Close（关闭）"
    //                                                  style:UIAlertActionStyleDefault
    //                                                handler:^(UIAlertAction * _Nonnull action) {/*点击按钮的响应事件；*/}]];
    //        //弹出提示框；
    //        [self presentViewController:alert animated:true completion:nil];
    //    }
    
    
    //NSString *str2=@"east2west_";
    //buyTypeReal=[str2 stringByAppendingString:buyType];
	
	buyTypeReal=buyType;
    NSString *strDes=[NSString stringWithUTF8String:c];
    [[CIAPClass sharedManager] buy:buyTypeReal cost:price purchaseID:buyTypeReal purchaseDiscribe:strDes];
}

- (void)paymentQueue:(SKPaymentQueue *)queue updatedTransactions:(NSArray *)transactions//交易结果
{
    NSLog(@"-----paymentQueue--------");
    for (SKPaymentTransaction *transaction in transactions)
    {
        switch (transaction.transactionState)
        {
            case SKPaymentTransactionStatePurchased://交易完成
            {
                [self completeTransaction:transaction];
                NSLog(@"-----交易完成 --------");
                /*UIAlertView *alerView =  [[UIAlertView alloc] initWithTitle:@"提示"
                 message:@"购买成功！"
                 delegate:nil cancelButtonTitle:NSLocalizedString(@"Close（关闭）",nil) otherButtonTitles:nil];
                 [alerView show];
                 */
                
                UIAlertController *alert = [UIAlertController alertControllerWithTitle:@"提示"
                                                                               message:@"购买成功！"
                                                                        preferredStyle:  UIAlertControllerStyleAlert];
                [alert addAction:[UIAlertAction actionWithTitle:@"Close（关闭）"
                                                          style:UIAlertActionStyleDefault
                                                        handler:^(UIAlertAction * _Nonnull action) {/*点击按钮的响应事件；*/}]];
                UIViewController *top = [UIApplication sharedApplication].keyWindow.rootViewController;
                //弹出提示框；
                [top presentViewController:alert animated:true completion:nil];
                
                
                //购买成功之后给用户发放道具
                NSDateFormatter *formatter = [[NSDateFormatter alloc]init]; //初始化格式器。
                [formatter setDateFormat:@"YYYY-MM-dd hh:mm:ss"];//定义时间为这种格式： YYYY-MM-dd hh:mm:ss 。
                NSString *currentTime = [formatter stringFromDate:[NSDate date]];//将NSDate  ＊对象 转化为 NSString
                UnitySendMessage("DontDestroy_Qin","BackUpOrderid",[currentTime UTF8String]);
                UnitySendMessage("DontDestroy_Qin","PaySeccess",[buyType UTF8String]);
                
                // const char* item = [buyType cStringUsingEncoding:NSASCIIStringEncoding];
                //int index = [self getOrderIndex:buyType];
            }
                break;
            case SKPaymentTransactionStateFailed://交易失败
            {
                [self failedTransaction:transaction];
                NSLog(@"-----交易失败 --------");
                NSString *temp=@"...";
                UnitySendMessage("Menu_Shop","PayFailed",[temp UTF8String]);
                /*UIAlertView *alerView2 =  [[UIAlertView alloc] initWithTitle:@"提示"
                 message:@"购买失败，请重新尝试购买！"
                 delegate:nil cancelButtonTitle:NSLocalizedString(@"Close（关闭）",nil) otherButtonTitles:nil];
                 [alerView2 show];
                 */
                UIAlertController *alert = [UIAlertController alertControllerWithTitle:@"提示"
                                                                               message:@"购买失败，请重新尝试购买！"
                                                                        preferredStyle:  UIAlertControllerStyleAlert];
                [alert addAction:[UIAlertAction actionWithTitle:@"Close（关闭）"
                                                          style:UIAlertActionStyleDefault
                                                        handler:^(UIAlertAction * _Nonnull action) {/*点击按钮的响应事件；*/}]];
                //弹出提示框；
                UIViewController *top = [UIApplication sharedApplication].keyWindow.rootViewController;
                [top presentViewController:alert animated:true completion:nil];
            }
                break;
            case SKPaymentTransactionStateRestored://已经购买过该商品
            {
                //[self restoreTransaction:transaction];
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
-(void)RequestProductData
{
    NSLog(@"---------请求对应的产品信息------------");
    //NSString *str2=@"east2west_";
    //buyTypeReal=[str2 stringByAppendingString:buyType];
	
	buyTypeReal=buyType;
    NSSet *set = [NSSet setWithObjects:buyTypeReal, nil];
    SKProductsRequest *request=[[SKProductsRequest alloc] initWithProductIdentifiers: set];
    request.delegate=self;
    [request start];
    
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
- (void) failedTransaction: (SKPaymentTransaction *)transaction{
    NSLog(@"失败");
    if (transaction.error.code != SKErrorPaymentCancelled)
    {
    }
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
    
    
    
    //SKPayment *payment = [SKPayment paymentWithProduct:buyType];
    //NSLog(@"---------发送购买请求------------");
    //[[SKPaymentQueue defaultQueue] addPayment:payment];
    ////    [request autorelease];
    
    
    
    
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
        
        if([pro.productIdentifier isEqualToString:buyTypeReal]){
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

- (void)showLOG:(NSString *)logMessage{
    NSLog(@"%@",logMessage);
}

@end




