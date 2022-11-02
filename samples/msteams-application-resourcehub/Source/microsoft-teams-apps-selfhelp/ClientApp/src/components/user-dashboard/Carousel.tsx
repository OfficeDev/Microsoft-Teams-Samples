import { ChevronEndMediumIcon, ChevronStartIcon } from '@fluentui/react-northstar'
import React, { useEffect, useState } from 'react'
import './carousel.scss'

const Carousel = (props) => {
    const {children, show,isScenario} = props

    const [currentIndex, setCurrentIndex] = useState(0)
    const [length, setLength] = useState(children.length)

    const [touchPosition, setTouchPosition] = useState(null)

    // Set the length to match current children from props
    useEffect(() => {
        setLength(children.length)
    }, [children])

    const next = () => {
        if (currentIndex < (length - show)) {
            setCurrentIndex(prevState => prevState + 1)
        }
    }

    const prev = () => {
        if (currentIndex > 0) {
            setCurrentIndex(prevState => prevState - 1)
        }
    }

    const handleTouchStart = (e) => {
        const touchDown = e.touches[0].clientX
        setTouchPosition(touchDown)
    }

    const handleTouchMove = (e) => {
        const touchDown = touchPosition

        if(touchDown === null) {
            return
        }

        const currentTouch = e.touches[0].clientX
        const diff = touchDown - currentTouch

        if (diff > 19) {
            next()
        }

        if (diff < -19) {
            prev()
        }

        setTouchPosition(null)
    }

    return (
        <>
            {length > 4 ?
        <div className={isScenario ==true?`carousel-container-scenario`:"carousel-container"}>
            <div className={isScenario ==true?"carousel-wrapper-scenario":"carousel-wrapper"}>
                {/* You can alwas change the content of the button to other things */}
                {
                    currentIndex > 0 &&
                    <ChevronStartIcon outline size="medium" className={isScenario ==true?"left-arrow-scenario":"left-arrow"} onClick={prev} />
                }
                <div
                    className={isScenario ==true?"carousel-content-wrapper-scenario":"carousel-content-wrapper"}
                    onTouchStart={handleTouchStart}
                    onTouchMove={handleTouchMove}
                >
                    <div
                        className={isScenario ==true?`carousel-content show-scenario-${show}`:`carousel-content show-${show}`}
                        style={{ transform: `translateX(-${currentIndex * (100 / show)}%)` }}
                    >
                        {children}
                    </div>
                </div>
                {/* You can alwas change the content of the button to other things */}
                {
                    currentIndex < (length - show) &&
                    <ChevronEndMediumIcon outline size="medium" className={isScenario ==true?"right-arrow-scenario":"right-arrow"} onClick={next} />
                }
            </div>
        </div>: <div className={isScenario ==true?`carousel-container-scenario-${length}`:"carousel-container"}>
        <div className={isScenario ==true?"carousel-wrapper-scenario":"carousel-wrapper"}>
                {/* You can alwas change the content of the button to other things */}
                {
                    currentIndex > 0 &&
                    <ChevronStartIcon outline size="medium" className={isScenario ==true?"left-arrow-scenario":"left-arrow"} onClick={prev} />
                }
                <div
                    className={isScenario ==true?"carousel-content-wrapper-scenario":"carousel-content-wrapper"}
                    onTouchStart={handleTouchStart}
                    onTouchMove={handleTouchMove}
                >
                    <div
                        className={isScenario ==true?`carousel-content show-scenario-${show}`:`carousel-content show-${show}`}
                        style={{ transform: `translateX(-${currentIndex * (100 / show)}%)` }}
                    >
                        {children}
                    </div>
                </div>
                {/* You can alwas change the content of the button to other things */}
                {
                    currentIndex < (length - show) &&
                    <ChevronEndMediumIcon outline size="medium" className={isScenario ==true?"right-arrow-scenario":"right-arrow"} onClick={next} />
                }
            </div>
        </div>
}
</>

    )
}

export default Carousel