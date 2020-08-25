var scene, camera, renderer;

var WIDTH  = 400;
var HEIGHT = 400;
var pi = 3.14159265359;
var SPEED = 0.001;

function init() {
    scene = new THREE.Scene();
    var xyz = window.location.hash.split("#")[1].split(',')
    initCube();
    initCamera();
    initRenderer();
    var axesHelper = new THREE.AxesHelper( 2 );
    scene.add( axesHelper );

    cube.rotation.x = THREE.Math.degToRad(xyz[0]);
    cube.rotation.y = THREE.Math.degToRad(xyz[1]);
    cube.rotation.z = THREE.Math.degToRad(xyz[2]);
    document.body.appendChild(renderer.domElement);
}

function initCamera() {
    camera = new THREE.PerspectiveCamera(70, WIDTH / HEIGHT, 1, 10);
    camera.position.set(0, 0, 4);
    camera.lookAt(scene.position);
}

function initRenderer() {
    renderer = new THREE.WebGLRenderer({ antialias: true });
    renderer.setSize(WIDTH, HEIGHT);
}

function initCube() {
    cube = new THREE.Mesh(new THREE.CubeGeometry(1, 1, 3), new THREE.MeshNormalMaterial());
    
    scene.add(cube);
}



function render() {
    requestAnimationFrame(render);
    renderer.render(scene, camera);
}

init();
render();
