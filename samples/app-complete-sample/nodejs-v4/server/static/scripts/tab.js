import * as fs from 'fs/promises';
const isFile = fileName => {
  return fs.lstatSync(fileName).isFile()
}
  function getFileDetails(){
    debugger;
    const dir = './server/dialogs';
    // list all files in the directory
      
      fs.readdirSync(dir).map(fileName => {
        resultEle.innerHTML += path.join(dir, fileName);
          console.log(path.join(dir, fileName));
        //return path.join(dir, fileName)
      })
      .filter(isFile)

      fs.readFile('./server/dialogs/basic/helloDialog.js', (err, data) => {
        if (err) throw err;
        filecontent.innerHTML=data.toString();
        console.log(data.toString());
    })

    let resultEle = document.querySelector(".result");
    let filecontent = document.querySelector("#filecontent");
  }

  document.querySelector('.Btn').addEventListener('click',()=>{
    alert("hello");
    getFileDetails();
 });