const fs = require('fs');
['/tmp/e2e_middleware.log','/tmp/e2e_middleware_raw.log','/tmp/e2e_middleware_decoded.log','/tmp/e2e_request_user.log'].forEach(p=>{
  try{
    const txt = fs.readFileSync(p,'utf8');
    console.log('---',p,'---');
    console.log(txt);
  }catch(e){
    console.log('---',p,'MISSING/ERR---',e.message);
  }
});
