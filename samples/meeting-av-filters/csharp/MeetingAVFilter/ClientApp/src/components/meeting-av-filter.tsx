import React from 'react';
import { app, video  } from "@microsoft/teams-js";
import "./configure-message.css"
import { WebglVideoFilter } from "./webgl-video-filter";

const MeetingAvFilter = () => {

    React.useEffect(() => {
        app.initialize().then(() => {
            // This is the effect for processing
            let appliedEffect = {
                pixelValue: 100,
                proportion: 3,
            };

            // These are the id's which we have defined inside the manifest for different effects.
            let effectIds = {
                half: "c2cf81fd-a1c0-4742-b41a-ef969b3ed490",
                gray: "b0c8896c-7be8-4645-ae02-a8bc9b0355e5",
            }

            // This is the effect linked with UI
            let selectedEffectId: any = undefined;

            // Process video frames for effect 1
            function simpleHalfEffect(videoFrame: any) {
                const maxLen =
                    (videoFrame.height * videoFrame.width) /
                    Math.max(1, appliedEffect.proportion) - 4;

                for (let i = 1; i < maxLen; i += 4) {
                    //samaple effect just change the value to 100, which effect some pixel value of video frame
                    videoFrame.data[i + 1] = appliedEffect.pixelValue;
                }
            }

            // Create a canvas for processing video frames.
            let canvas = new OffscreenCanvas(480, 360);
            let videoFilter = new WebglVideoFilter(canvas);
            videoFilter.init();

            // Process video frames for effect 2
            function videoFrameHandler(videoFrame: any, notifyVideoProcessed: any, notifyError: any) {
                switch (selectedEffectId) {
                    case effectIds.half:
                        simpleHalfEffect(videoFrame);
                        break;
                    case effectIds.gray:
                        videoFilter.processVideoFrame(videoFrame);
                        break;
                    default:
                        break;
                }

                //send notification the effect processing is finshed.
                notifyVideoProcessed();
            }

            // Clear the selected effect
            function clearSelect() {
                document.getElementById("filter-half")!.classList.remove("selected");
                document.getElementById("filter-gray")!.classList.remove("selected");
            }

            // This method is invoked when filter is changed
            function effectParameterChanged(effectId: any) {
                alert("effect changed" + effectId);
                console.log(effectId);
                if (selectedEffectId === effectId) {
                    console.log('effect not changed');
                    return;
                }
                selectedEffectId = effectId;

                // Clear the css for selected filter.
                clearSelect();
                switch (selectedEffectId) {
                    case effectIds.half:
                        console.log('current effect: half');
                        document.getElementById("filter-half")!.classList.add("selected");
                        break;
                    case effectIds.gray:
                        console.log('current effect: gray');
                        document.getElementById("filter-gray")!.classList.add("selected");
                        break;
                    default:
                        console.log('effect cleared');
                        break;
                }
            }

            // Register an handler to handle effect change event
            video.registerForVideoEffect(effectParameterChanged);

            // Register an handler to handle video frames
            video.registerForVideoFrame(videoFrameHandler, {
                format: 0,
            });

            // Any changes to the UI filter should be notified back to Teams client.
            const filterHalf = document.getElementById("filter-half");
            filterHalf!.addEventListener("click", function () {
                if (selectedEffectId === effectIds.half) {
                    return;
                }

                video.notifySelectedVideoEffectChanged(0, effectIds.half);
            });

            const filterGray = document.getElementById("filter-gray");
            filterGray!.addEventListener("click", function () {
                if (selectedEffectId === effectIds.gray) {
                    return;
                }

                video.notifySelectedVideoEffectChanged(0, effectIds.gray);
            });
        });
    }, [])

    return (
        <>
            <h1 className="app-title">Video app sample</h1>
            <div className="horizontal">
                <div className="filter" id="filter-half">
                    <a className="thumbnail"></a>
                </div>
                <div className="filter" id="filter-gray">
                    <a className="thumbnail"></a>
                </div>
            </div>
        </>
    )
}

export default (MeetingAvFilter);