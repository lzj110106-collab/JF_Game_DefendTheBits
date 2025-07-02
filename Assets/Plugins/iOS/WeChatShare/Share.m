//
//  Share.m
//  Unity-iPhone
//
//  Created by Qiang Qiang on 2018/8/30.
//
#import "Share.h"
#import "WXApi.h"

#import <Foundation/Foundation.h>


//分享到会话
void ShareToWeChat(Byte imageData[],int shareType,int ArrayLength,int approach){
//shareType:分享类型，主界面分享固定的宣传图，其他界面分享实时截图
//0：固定分享，1：实时截图分享
    if(shareType==1){
        NSData *adata=[[NSData alloc] initWithBytes:imageData length:ArrayLength];
        
        //UIImage *aimage=[UIImage imageWithData:adata];
        
        
        WXMediaMessage *messgae=[WXMediaMessage message];
        //[messgae setThumbImage:aimage];
        //[messgae setThumbImage:[UIImage imageNamed:@"81.png"]];
        
        WXImageObject *imageObject=[WXImageObject object];
        imageObject.imageData=adata;
        messgae.mediaObject=imageObject;
        
        SendMessageToWXReq *req=[[SendMessageToWXReq alloc] init];
        req.bText=NO;
        req.message=messgae;
        if(approach==0)
            req.scene=WXSceneSession;
        else
            req.scene=WXSceneTimeline;
        
        [WXApi sendReq:req];
        
    }else{
            WXMediaMessage *message = [WXMediaMessage message];
            [message setThumbImage:[UIImage imageNamed:@"81.png"]];
            //缩略图
            WXImageObject *imageObject = [WXImageObject object];
            NSString *filePath = [[NSBundle mainBundle]
                                  pathForResource:@"81" ofType:@"jpg"];//图片路径
            imageObject.imageData = [NSData dataWithContentsOfFile:filePath];
            message.mediaObject = imageObject;
        
            SendMessageToWXReq *req = [[SendMessageToWXReq alloc] init];
            req.bText = NO;
            req.message = message;
        if(approach==0)
            req.scene=WXSceneSession;
        else
            req.scene=WXSceneTimeline;
        
        [WXApi sendReq:req];
    }
    
    
}
