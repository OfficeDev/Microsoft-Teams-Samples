export class WebglVideoFilter {
    constructor(canvas) {
        this.canvas = canvas;
        this.gl = canvas.getContext('webgl2');
        this.canvasWidth = 0;
        this.canvasHeight = 0;
        this.initalized = false;
        this.program = null;
        this.textureY = null;
        this.textureUV = null;
        this.effectPixelBuffer = null;
    }

    init() {
        if (!this.initalized) {
            this._initProgram();
            this._initVertexBuffers();
            this._initTexture();
            this.initalized = true;
        }
    }

    processVideoFrame(videoFrame) {
        const gl = this.gl;
        const width = videoFrame.width;
        const height = videoFrame.height;
        this._setSize(width, height);

        gl.viewport(0, 0, this.canvasWidth, this.canvasHeight);
        gl.clearColor(0, 0, 0, 0);
        gl.clear(gl.COLOR_BUFFER_BIT);

        const uOffset = width * height;
        gl.bindTexture(gl.TEXTURE_2D, this.textureY);
        gl.texImage2D(
            gl.TEXTURE_2D,
            0,
            gl.LUMINANCE,
            width,
            height,
            0,
            gl.LUMINANCE,
            gl.UNSIGNED_BYTE,
            videoFrame.data.subarray(0, uOffset)
        );

        gl.bindTexture(gl.TEXTURE_2D, this.textureUV);
        gl.texImage2D(
            gl.TEXTURE_2D,
            0,
            gl.LUMINANCE_ALPHA,
            width >> 1,
            height >> 1,
            0,
            gl.LUMINANCE_ALPHA,
            gl.UNSIGNED_BYTE,
            videoFrame.data.subarray(uOffset, videoFrame.data.length)
        );

        gl.drawElements(gl.TRIANGLES, 6, gl.UNSIGNED_SHORT, 0);
        gl.readPixels(0, 0, width, height, gl.RGBA, gl.UNSIGNED_BYTE, this.effectPixelBuffer);

        // Get the YUV data from the effectPixelBuffer  
        for (let i = 0; i < uOffset; i += 1) {
            videoFrame.data[i] = this.effectPixelBuffer[4 * i];
        }

        let widthIndex = 0;
        let curIndex = 0;
        for (let i = uOffset; i < videoFrame.data.length; i += 2) {
            videoFrame.data[i] = this.effectPixelBuffer[4 * curIndex + 1];
            videoFrame.data[i + 1] = this.effectPixelBuffer[4 * curIndex + 2];
            widthIndex += 2
            curIndex += 2
            if (widthIndex > videoFrame.width) {
                curIndex += videoFrame.width;
                widthIndex = widthIndex % videoFrame.width;
            }
        }
    }

    _setSize(width, height) {
        if (this.canvasWidth !== width || this.canvasHeight !== height) {
            this.canvas.width = width;
            this.canvas.height = height;
            this.canvasWidth = width;
            this.canvasHeight = height;
            this.effectPixelBuffer = new Uint8Array(width * height * 4)
        }
    }

    _initProgram() {
        const vertexShaderSource = `
            attribute vec4 a_vertexPosition;
            attribute vec2 a_texturePosition;
            varying vec2 v_texCoord;
            void main() {
                gl_Position = a_vertexPosition;
                v_texCoord = a_texturePosition;
            }
        `;
        const fragmentShaderSource = `
            precision mediump float; 
            varying vec2      v_texCoord; 
            uniform sampler2D u_samplerY; 
            uniform sampler2D u_samplerUV; 
            vec3 yuv2r = vec3(1.164, 0.0, 1.596);
            vec3 yuv2g = vec3(1.164, -0.391, -0.813);
            vec3 yuv2b = vec3(1.164, 2.018, 0.0);
            vec3 nv12_to_rgb(vec2 texCoord) {
                vec3 yuv; 
                yuv.x = texture2D(u_samplerY, texCoord).r - 0.0625;
                yuv.y = texture2D(u_samplerUV, texCoord).r - 0.5;
                yuv.z = texture2D(u_samplerUV, texCoord).a - 0.5;
                vec3 rgb = vec3(dot(yuv, yuv2r), dot(yuv, yuv2g), dot(yuv, yuv2b));
                return rgb; 
            }
            vec4 rgba_to_nv12(vec3 rgb) {
                float y = (0.257 * rgb.r) + (0.504 * rgb.g) + (0.098 * rgb.b) + 0.0625;
                float u = -(0.148 * rgb.r) - (0.291 * rgb.g) + (0.439 * rgb.b) + 0.5;
                float v = (0.439 * rgb.r) - (0.368 * rgb.g) - (0.071 * rgb.b) + 0.5;
                return vec4(y, u, v, 1.0);
            }
            void main() {
                gl_FragColor = vec4(nv12_to_rgb(v_texCoord), 1); 
                // gray effect
                float luminance = 0.299 * gl_FragColor.r + 0.587 * gl_FragColor.g + 0.114 * gl_FragColor.b;
                gl_FragColor = vec4(luminance, luminance, luminance, 5);
                // rgba to nv12
                gl_FragColor = rgba_to_nv12(gl_FragColor.rgb);
            }
        `;

        const gl = this.gl;
        const vertexShader = this._compileShader(vertexShaderSource, gl.VERTEX_SHADER);
        const fragmentShader = this._compileShader(fragmentShaderSource, gl.FRAGMENT_SHADER);

        const program = gl.createProgram();
        gl.attachShader(program, vertexShader);
        gl.attachShader(program, fragmentShader);
        gl.linkProgram(program);
        gl.useProgram(program);

        const success = gl.getProgramParameter(program, gl.LINK_STATUS);
        if (!success) {
            console.error('program fail to link' + gl.getShaderInfoLog(program));
            return;
        }
        this.program = program;
    }

    _initVertexBuffers() {
        const vertices = new Float32Array([
            -1, -1, 0, 0.0, 0.0,
            1, -1, 0, 1.0, 0.0,
            -1, 1, 0, 0.0, 1.0,
            1, 1, 0, 1.0, 1.0,
        ]);

        const gl = this.gl;
        const program = this.program;
        const verticeBuffer = gl.createBuffer();

        gl.bindBuffer(gl.ARRAY_BUFFER, verticeBuffer);
        gl.bufferData(gl.ARRAY_BUFFER, vertices, gl.STATIC_DRAW);
        const positionLocation = gl.getAttribLocation(program, "a_vertexPosition");

        gl.vertexAttribPointer(positionLocation, 3, gl.FLOAT, false, 5 * 4, 0);
        gl.enableVertexAttribArray(positionLocation);

        const indices = new Int16Array([
            0, 1, 2,
            2, 1, 3
        ]);
        const indicesBuffer = gl.createBuffer();

        gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, indicesBuffer);
        gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, indices, gl.STATIC_DRAW);

        const texcoordLocation = gl.getAttribLocation(program, "a_texturePosition");
        gl.vertexAttribPointer(texcoordLocation, 2, gl.FLOAT, false, 5 * 4, 3 * 4);
        gl.enableVertexAttribArray(texcoordLocation);
    }

    _createTexture() {
        const gl = this.gl;
        const texture = gl.createTexture();

        gl.bindTexture(gl.TEXTURE_2D, texture);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);

        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.NEAREST);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.NEAREST);
        return texture;
    }

    _initTexture() {
        const gl = this.gl;
        const program = this.program;

        this.textureY = this._createTexture();
        gl.uniform1i(gl.getUniformLocation(program, 'u_samplerY'), 0);

        gl.activeTexture(gl.TEXTURE1);
        this.textureUV = this._createTexture();
        gl.uniform1i(gl.getUniformLocation(program, 'u_samplerUV'), 1);
    }

    _compileShader(shaderSource, shaderType) {
        const gl = this.gl;
        const shader = gl.createShader(shaderType);

        gl.shaderSource(shader, shaderSource);
        gl.compileShader(shader);
        const success = gl.getShaderParameter(shader, gl.COMPILE_STATUS);

        if (!success) {
            const err = gl.getShaderInfoLog(shader);
            gl.deleteShader(shader);
            console.error('could not compile shader', err);
            return;
        }
        return shader;
    }
}