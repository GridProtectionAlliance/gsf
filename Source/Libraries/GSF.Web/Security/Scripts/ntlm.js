// NTLM (ntlm.js) authentication in JavaScript.
// ------------------------------------------------------------------------
// The MIT License (MIT). Copyright (c) 2012 Erland Ranvinge.
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

// WARNING: This file has been modified from the original work:
// Ntlm.authenticate method has been updated to operate asynchronously

var Msg = function(data) {
    this.data = [];
    if (!data) return;
    if (data.indexOf("NTLM ") === 0) data = data.substr(5);
    atob(data).split("").map(function(c) { this.push(c.charCodeAt(0)); }, this.data);
};

Msg.prototype.addByte = function(b) {
    this.data.push(b);
};

Msg.prototype.addShort = function(s) {
    this.data.push(s & 0xFF);
    this.data.push((s >> 8) & 0xFF);
};

Msg.prototype.addString = function(str, utf16) {
    if (utf16) // Fake UTF16 by padding each character in string.
        str = str.split("").map(function(c) { return (c + "\0"); }).join("");

    for (let i = 0; i < str.length; i++)
        this.data.push(str.charCodeAt(i));
};

Msg.prototype.getString = function(offset, length) {
    var result = "";
    for (let i = 0; i < length; i++) {
        if (offset + i >= this.data.length)
            return "";
        result += String.fromCharCode(this.data[offset + i]);
    }
    return result;
};

Msg.prototype.getByte = function(offset) {
    return this.data[offset];
};

Msg.prototype.toBase64 = function() {
    const str = String.fromCharCode.apply(null, this.data);
    return btoa(str).replace(/.{76}(?=.)/g,"$&");
};

var Ntlm = {};

Ntlm.domain = null;
Ntlm.username = null;
Ntlm.lmHashedPassword = null;
Ntlm.ntHashedPassword = null;

Ntlm.error = function(msg) {
    console.error(msg);
};

Ntlm.message = function(msg) {
    console.log(msg);
};

Ntlm.createMessage1 = function(hostname) {
    const msg1 = new Msg();
    msg1.addString("NTLMSSP\0");
    msg1.addByte(1);
    msg1.addString("\0\0\0");
    msg1.addShort(0xb203);
    msg1.addString("\0\0");
    msg1.addShort(Ntlm.domain.length);
    msg1.addShort(Ntlm.domain.length);
    msg1.addShort(32 + hostname.length);
    msg1.addString("\0\0");
    msg1.addShort(hostname.length);
    msg1.addShort(hostname.length);
    msg1.addShort(32);
    msg1.addString("\0\0");
    msg1.addString(hostname.toUpperCase());
    msg1.addString(Ntlm.domain.toUpperCase());
    return msg1;
};

Ntlm.getChallenge = function(data) {
    const msg2 = new Msg(data);
    
    if (msg2.getString(0, 8) !== "NTLMSSP\0") {
        Ntlm.error("Invalid NTLM response header.");
        return "";
    }
    
    if (msg2.getByte(8) !== 2) {
        Ntlm.error("Invalid NTLM response type.");
        return "";
    }
    
    const challenge = msg2.getString(24, 8);
    
    return challenge;
};

Ntlm.createMessage3 = function(challenge, hostname) {
    var lmResponse = Ntlm.buildResponse(Ntlm.lmHashedPassword, challenge);
    var ntResponse = Ntlm.buildResponse(Ntlm.ntHashedPassword, challenge);
    var username = Ntlm.username;
    var domain = Ntlm.domain;
    var msg3 = new Msg();

    msg3.addString("NTLMSSP\0");
    msg3.addByte(3);
    msg3.addString("\0\0\0");

    msg3.addShort(24); // lmResponse
    msg3.addShort(24);
    msg3.addShort(64 + (domain.length + username.length + hostname.length) * 2);
    msg3.addString("\0\0");

    msg3.addShort(24); // ntResponse
    msg3.addShort(24);
    msg3.addShort(88 + (domain.length + username.length + hostname.length) * 2);
    msg3.addString("\0\0");

    msg3.addShort(domain.length * 2); // Domain.
    msg3.addShort(domain.length * 2);
    msg3.addShort(64);
    msg3.addString("\0\0");

    msg3.addShort(username.length * 2); // Username.
    msg3.addShort(username.length * 2);
    msg3.addShort(64 + domain.length * 2);
    msg3.addShort("\0\0");

    msg3.addShort(hostname.length * 2); // Hostname.
    msg3.addShort(hostname.length * 2);
    msg3.addShort(64 + (domain.length + username.length) * 2);
    msg3.addString("\0\0");

    msg3.addString("\0\0\0\0");
    msg3.addShort(112 + (domain.length + username.length + hostname.length) * 2);
    msg3.addString("\0\0");
    msg3.addShort(0x8201);
    msg3.addString("\0\0");

    msg3.addString(domain.toUpperCase(), true); // "Some" string are passed as UTF-16.
    msg3.addString(username, true);
    msg3.addString(hostname.toUpperCase(), true);
    msg3.addString(lmResponse);
    msg3.addString(ntResponse);

    return msg3;
};

Ntlm.createKey = function(str) {
    const key56 = [];

    while (str.length < 7) str += "\0";

    str = str.substr(0, 7);
    str.split("").map(function(c) { this.push(c.charCodeAt(0)); }, key56);

    const key = [0, 0, 0, 0, 0, 0, 0, 0];

    key[0] = key56[0]; // Convert 56 bit key to 64 bit.
    key[1] = ((key56[0] << 7) & 0xFF) | (key56[1] >> 1);
    key[2] = ((key56[1] << 6) & 0xFF) | (key56[2] >> 2);
    key[3] = ((key56[2] << 5) & 0xFF) | (key56[3] >> 3);
    key[4] = ((key56[3] << 4) & 0xFF) | (key56[4] >> 4);
    key[5] = ((key56[4] << 3) & 0xFF) | (key56[5] >> 5);
    key[6] = ((key56[5] << 2) & 0xFF) | (key56[6] >> 6);
    key[7] =  (key56[6] << 1) & 0xFF;

    for (let i = 0; i < key.length; i++) { // Fix DES key parity bits.
        let bit = 0;

        for (let k = 0; k < 7; k++) {
            const t = key[i] >> k;
            bit = (t ^ bit) & 0x1;
        }

        key[i] = (key[i] & 0xFE) | bit;
    }

    var result = "";

    key.map(function(i) { result += String.fromCharCode(i); });

    return result;
};

Ntlm.buildResponse = function(key, text) {
    while (key.length < 21)
        key += "\0";

    const key1 = Ntlm.createKey(key.substr(0, 7));
    const key2 = Ntlm.createKey(key.substr(7, 7));
    const key3 = Ntlm.createKey(key.substr(14, 7));

    return des(key1, text, 1, 0) + des(key2, text, 1, 0) + des(key3, text, 1, 0);
};

Ntlm.getLocation = function(url) {
    const l = document.createElement("a");
    l.href = url;
    return l;
};

Ntlm.setCredentials = function(domain, username, password) {
    const magic = "KGS!@#$%"; // Create LM password hash.
    var lmPassword = password.toUpperCase().substr(0, 14);
    while (lmPassword.length < 14) lmPassword += "\0";
    const key1 = Ntlm.createKey(lmPassword);
    const key2 = Ntlm.createKey(lmPassword.substr(7));
    const lmHashedPassword = des(key1, magic, 1, 0) + des(key2, magic, 1, 0);

    var ntPassword = ""; // Create NT password hash.
    for (let i = 0; i < password.length; i++)
        ntPassword += password.charAt(i) + "\0";
    const ntHashedPassword = str_md4(ntPassword);

    Ntlm.domain = domain;
    Ntlm.username = username;
    Ntlm.lmHashedPassword = lmHashedPassword;
    Ntlm.ntHashedPassword = ntHashedPassword;
};

Ntlm.isChallenge = function(xhr) {
    if (!xhr)
        return false;
    
    if (xhr.status !== 401)
        return false;
    
    const header = xhr.getResponseHeader("WWW-Authenticate");
    return header && header.indexOf("NTLM") !== -1;
};

Ntlm.authenticate = function (url, handleResponse, verificationHeader, verificationValue, useAjaxVerfication) {
    if (!Ntlm.domain || !Ntlm.username || !Ntlm.lmHashedPassword || !Ntlm.ntHashedPassword) {
        Ntlm.error("No NTLM credentials specified. Use Ntlm.setCredentials(...) before making calls.");
        return;
    }

    if (url.startsWith("/"))
        url = window.location.protocol + "//" + window.location.host + url;

    var hostname = Ntlm.getLocation(url).hostname;

    if (!hostname)
        hostname = window.location.hostname;
    
    $.ajax({
        url: url,
        method: "post",
        cache: false,
        crossDomain: false,
        xhrFields: {
            withCredentials: true
        },
        complete: function (xhr) {
            switch (xhr.status) {
                case 401:
                    $.ajax({
                        url: url,
                        method: "post",
                        cache: false,
                        crossDomain: false,
                        xhrFields: {
                            withCredentials: true
                        },
                        complete: function (xhr2) {
                            handleResponse(xhr2.statusCode());
                        },
                        beforeSend: function (xhr2) {
                            const response = xhr.getResponseHeader("WWW-Authenticate");
                            const challenge = Ntlm.getChallenge(response);
                            const msg3 = Ntlm.createMessage3(challenge, hostname);

                            xhr2.setRequestHeader("Authorization", "NTLM " + msg3.toBase64());

                            if (verificationHeader && verificationValue) {
                                xhr2.setRequestHeader(verificationHeader, verificationValue);
                                xhr2.setRequestHeader(useAjaxVerfication, "true");
                            }
                        }
                    });
                    break;
                default:
                    handleResponse(xhr.statusCode());
                    break;
            }
        },
        beforeSend: function (xhr) {
            const msg1 = Ntlm.createMessage1(hostname);
            xhr.setRequestHeader("Authorization", "NTLM " + msg1.toBase64());

            if (verificationHeader && verificationValue) {
                xhr.setRequestHeader(verificationHeader, verificationValue);
                xhr.setRequestHeader(useAjaxVerfication, "true");
            }
        }
    });
};

/*
 * A JavaScript implementation of the RSA Data Security, Inc. MD4 Message
 * Digest Algorithm, as defined in RFC 1320.
 * Version 2.1 Copyright (C) Jerrad Pierce, Paul Johnston 1999 - 2002.
 * Other contributors: Greg Holt, Andrew Kepert, Ydnar, Lostinet
 * Distributed under the BSD License
 * See http://pajhome.org.uk/crypt/md5 for more info.
 */

var chrsz   = 8;

function str_md4(s){ return binl2str(core_md4(str2binl(s), s.length * chrsz));}

function core_md4(x, len)
{
    x[len >> 5] |= 0x80 << (len % 32);
    x[(((len + 64) >>> 9) << 4) + 14] = len;

    var a =  1732584193;
    var b = -271733879;
    var c = -1732584194;
    var d =  271733878;

    for(var i = 0; i < x.length; i += 16)
    {
        var olda = a;
        var oldb = b;
        var oldc = c;
        var oldd = d;

        a = md4_ff(a, b, c, d, x[i+ 0], 3 );
        d = md4_ff(d, a, b, c, x[i+ 1], 7 );
        c = md4_ff(c, d, a, b, x[i+ 2], 11);
        b = md4_ff(b, c, d, a, x[i+ 3], 19);
        a = md4_ff(a, b, c, d, x[i+ 4], 3 );
        d = md4_ff(d, a, b, c, x[i+ 5], 7 );
        c = md4_ff(c, d, a, b, x[i+ 6], 11);
        b = md4_ff(b, c, d, a, x[i+ 7], 19);
        a = md4_ff(a, b, c, d, x[i+ 8], 3 );
        d = md4_ff(d, a, b, c, x[i+ 9], 7 );
        c = md4_ff(c, d, a, b, x[i+10], 11);
        b = md4_ff(b, c, d, a, x[i+11], 19);
        a = md4_ff(a, b, c, d, x[i+12], 3 );
        d = md4_ff(d, a, b, c, x[i+13], 7 );
        c = md4_ff(c, d, a, b, x[i+14], 11);
        b = md4_ff(b, c, d, a, x[i+15], 19);

        a = md4_gg(a, b, c, d, x[i+ 0], 3 );
        d = md4_gg(d, a, b, c, x[i+ 4], 5 );
        c = md4_gg(c, d, a, b, x[i+ 8], 9 );
        b = md4_gg(b, c, d, a, x[i+12], 13);
        a = md4_gg(a, b, c, d, x[i+ 1], 3 );
        d = md4_gg(d, a, b, c, x[i+ 5], 5 );
        c = md4_gg(c, d, a, b, x[i+ 9], 9 );
        b = md4_gg(b, c, d, a, x[i+13], 13);
        a = md4_gg(a, b, c, d, x[i+ 2], 3 );
        d = md4_gg(d, a, b, c, x[i+ 6], 5 );
        c = md4_gg(c, d, a, b, x[i+10], 9 );
        b = md4_gg(b, c, d, a, x[i+14], 13);
        a = md4_gg(a, b, c, d, x[i+ 3], 3 );
        d = md4_gg(d, a, b, c, x[i+ 7], 5 );
        c = md4_gg(c, d, a, b, x[i+11], 9 );
        b = md4_gg(b, c, d, a, x[i+15], 13);

        a = md4_hh(a, b, c, d, x[i+ 0], 3 );
        d = md4_hh(d, a, b, c, x[i+ 8], 9 );
        c = md4_hh(c, d, a, b, x[i+ 4], 11);
        b = md4_hh(b, c, d, a, x[i+12], 15);
        a = md4_hh(a, b, c, d, x[i+ 2], 3 );
        d = md4_hh(d, a, b, c, x[i+10], 9 );
        c = md4_hh(c, d, a, b, x[i+ 6], 11);
        b = md4_hh(b, c, d, a, x[i+14], 15);
        a = md4_hh(a, b, c, d, x[i+ 1], 3 );
        d = md4_hh(d, a, b, c, x[i+ 9], 9 );
        c = md4_hh(c, d, a, b, x[i+ 5], 11);
        b = md4_hh(b, c, d, a, x[i+13], 15);
        a = md4_hh(a, b, c, d, x[i+ 3], 3 );
        d = md4_hh(d, a, b, c, x[i+11], 9 );
        c = md4_hh(c, d, a, b, x[i+ 7], 11);
        b = md4_hh(b, c, d, a, x[i+15], 15);

        a = safe_add(a, olda);
        b = safe_add(b, oldb);
        c = safe_add(c, oldc);
        d = safe_add(d, oldd);

    }
    return Array(a, b, c, d);
}

function md4_cmn(q, a, b, x, s, t)
{
    return safe_add(rol(safe_add(safe_add(a, q), safe_add(x, t)), s), b);
}

function md4_ff(a, b, c, d, x, s)
{
    return md4_cmn((b & c) | ((~b) & d), a, 0, x, s, 0);
}

function md4_gg(a, b, c, d, x, s)
{
    return md4_cmn((b & c) | (b & d) | (c & d), a, 0, x, s, 1518500249);
}

function md4_hh(a, b, c, d, x, s)
{
    return md4_cmn(b ^ c ^ d, a, 0, x, s, 1859775393);
}

function safe_add(x, y)
{
    const lsw = (x & 0xFFFF) + (y & 0xFFFF);
    const msw = (x >> 16) + (y >> 16) + (lsw >> 16);
    return (msw << 16) | (lsw & 0xFFFF);
}

function rol(num, cnt)
{
    return (num << cnt) | (num >>> (32 - cnt));
}

function str2binl(str)
{
    const bin = Array();
    const mask = (1 << chrsz) - 1;
    for(let i = 0; i < str.length * chrsz; i += chrsz)
        bin[i>>5] |= (str.charCodeAt(i / chrsz) & mask) << (i%32);
    return bin;
}

function binl2str(bin)
{
    var str = "";
    const mask = (1 << chrsz) - 1;
    for(let i = 0; i < bin.length * 32; i += chrsz)
        str += String.fromCharCode((bin[i>>5] >>> (i % 32)) & mask);
    return str;
}

//Paul Tero, July 2001
//http://www.tero.co.uk/des/
//
//Optimised for performance with large blocks by Michael Hayworth, November 2001
//http://www.netdealing.com
//
//THIS SOFTWARE IS PROVIDED "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
//IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
//ARE DISCLAIMED.  IN NO EVENT SHALL THE AUTHOR OR CONTRIBUTORS BE LIABLE
//FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
//DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
//OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
//HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
//LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
//OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
//SUCH DAMAGE.

//des
//this takes the key, the message, and whether to encrypt or decrypt
function des (key, message, encrypt, mode, iv, padding) {
    //declaring this locally speeds things up a bit
    var spfunction1 = new Array (0x1010400,0,0x10000,0x1010404,0x1010004,0x10404,0x4,0x10000,0x400,0x1010400,0x1010404,0x400,0x1000404,0x1010004,0x1000000,0x4,0x404,0x1000400,0x1000400,0x10400,0x10400,0x1010000,0x1010000,0x1000404,0x10004,0x1000004,0x1000004,0x10004,0,0x404,0x10404,0x1000000,0x10000,0x1010404,0x4,0x1010000,0x1010400,0x1000000,0x1000000,0x400,0x1010004,0x10000,0x10400,0x1000004,0x400,0x4,0x1000404,0x10404,0x1010404,0x10004,0x1010000,0x1000404,0x1000004,0x404,0x10404,0x1010400,0x404,0x1000400,0x1000400,0,0x10004,0x10400,0,0x1010004);
    var spfunction2 = new Array (-0x7fef7fe0,-0x7fff8000,0x8000,0x108020,0x100000,0x20,-0x7fefffe0,-0x7fff7fe0,-0x7fffffe0,-0x7fef7fe0,-0x7fef8000,-0x80000000,-0x7fff8000,0x100000,0x20,-0x7fefffe0,0x108000,0x100020,-0x7fff7fe0,0,-0x80000000,0x8000,0x108020,-0x7ff00000,0x100020,-0x7fffffe0,0,0x108000,0x8020,-0x7fef8000,-0x7ff00000,0x8020,0,0x108020,-0x7fefffe0,0x100000,-0x7fff7fe0,-0x7ff00000,-0x7fef8000,0x8000,-0x7ff00000,-0x7fff8000,0x20,-0x7fef7fe0,0x108020,0x20,0x8000,-0x80000000,0x8020,-0x7fef8000,0x100000,-0x7fffffe0,0x100020,-0x7fff7fe0,-0x7fffffe0,0x100020,0x108000,0,-0x7fff8000,0x8020,-0x80000000,-0x7fefffe0,-0x7fef7fe0,0x108000);
    var spfunction3 = new Array (0x208,0x8020200,0,0x8020008,0x8000200,0,0x20208,0x8000200,0x20008,0x8000008,0x8000008,0x20000,0x8020208,0x20008,0x8020000,0x208,0x8000000,0x8,0x8020200,0x200,0x20200,0x8020000,0x8020008,0x20208,0x8000208,0x20200,0x20000,0x8000208,0x8,0x8020208,0x200,0x8000000,0x8020200,0x8000000,0x20008,0x208,0x20000,0x8020200,0x8000200,0,0x200,0x20008,0x8020208,0x8000200,0x8000008,0x200,0,0x8020008,0x8000208,0x20000,0x8000000,0x8020208,0x8,0x20208,0x20200,0x8000008,0x8020000,0x8000208,0x208,0x8020000,0x20208,0x8,0x8020008,0x20200);
    var spfunction4 = new Array (0x802001,0x2081,0x2081,0x80,0x802080,0x800081,0x800001,0x2001,0,0x802000,0x802000,0x802081,0x81,0,0x800080,0x800001,0x1,0x2000,0x800000,0x802001,0x80,0x800000,0x2001,0x2080,0x800081,0x1,0x2080,0x800080,0x2000,0x802080,0x802081,0x81,0x800080,0x800001,0x802000,0x802081,0x81,0,0,0x802000,0x2080,0x800080,0x800081,0x1,0x802001,0x2081,0x2081,0x80,0x802081,0x81,0x1,0x2000,0x800001,0x2001,0x802080,0x800081,0x2001,0x2080,0x800000,0x802001,0x80,0x800000,0x2000,0x802080);
    var spfunction5 = new Array (0x100,0x2080100,0x2080000,0x42000100,0x80000,0x100,0x40000000,0x2080000,0x40080100,0x80000,0x2000100,0x40080100,0x42000100,0x42080000,0x80100,0x40000000,0x2000000,0x40080000,0x40080000,0,0x40000100,0x42080100,0x42080100,0x2000100,0x42080000,0x40000100,0,0x42000000,0x2080100,0x2000000,0x42000000,0x80100,0x80000,0x42000100,0x100,0x2000000,0x40000000,0x2080000,0x42000100,0x40080100,0x2000100,0x40000000,0x42080000,0x2080100,0x40080100,0x100,0x2000000,0x42080000,0x42080100,0x80100,0x42000000,0x42080100,0x2080000,0,0x40080000,0x42000000,0x80100,0x2000100,0x40000100,0x80000,0,0x40080000,0x2080100,0x40000100);
    var spfunction6 = new Array (0x20000010,0x20400000,0x4000,0x20404010,0x20400000,0x10,0x20404010,0x400000,0x20004000,0x404010,0x400000,0x20000010,0x400010,0x20004000,0x20000000,0x4010,0,0x400010,0x20004010,0x4000,0x404000,0x20004010,0x10,0x20400010,0x20400010,0,0x404010,0x20404000,0x4010,0x404000,0x20404000,0x20000000,0x20004000,0x10,0x20400010,0x404000,0x20404010,0x400000,0x4010,0x20000010,0x400000,0x20004000,0x20000000,0x4010,0x20000010,0x20404010,0x404000,0x20400000,0x404010,0x20404000,0,0x20400010,0x10,0x4000,0x20400000,0x404010,0x4000,0x400010,0x20004010,0,0x20404000,0x20000000,0x400010,0x20004010);
    var spfunction7 = new Array (0x200000,0x4200002,0x4000802,0,0x800,0x4000802,0x200802,0x4200800,0x4200802,0x200000,0,0x4000002,0x2,0x4000000,0x4200002,0x802,0x4000800,0x200802,0x200002,0x4000800,0x4000002,0x4200000,0x4200800,0x200002,0x4200000,0x800,0x802,0x4200802,0x200800,0x2,0x4000000,0x200800,0x4000000,0x200800,0x200000,0x4000802,0x4000802,0x4200002,0x4200002,0x2,0x200002,0x4000000,0x4000800,0x200000,0x4200800,0x802,0x200802,0x4200800,0x802,0x4000002,0x4200802,0x4200000,0x200800,0,0x2,0x4200802,0,0x200802,0x4200000,0x800,0x4000002,0x4000800,0x800,0x200002);
    var spfunction8 = new Array (0x10001040,0x1000,0x40000,0x10041040,0x10000000,0x10001040,0x40,0x10000000,0x40040,0x10040000,0x10041040,0x41000,0x10041000,0x41040,0x1000,0x40,0x10040000,0x10000040,0x10001000,0x1040,0x41000,0x40040,0x10040040,0x10041000,0x1040,0,0,0x10040040,0x10000040,0x10001000,0x41040,0x40000,0x41040,0x40000,0x10041000,0x1000,0x40,0x10040040,0x1000,0x41040,0x10001000,0x40,0x10000040,0x10040000,0x10040040,0x10000000,0x40000,0x10001040,0,0x10041040,0x40040,0x10000040,0x10040000,0x10001000,0x10001040,0,0x10041040,0x41000,0x41000,0x1040,0x1040,0x40040,0x10000000,0x10041000);

    //create the 16 or 48 subkeys we will need
    var keys = des_createKeys (key);
    var m=0, i, j, temp, temp2, right1, right2, left, right, looping;
    var cbcleft, cbcleft2, cbcright, cbcright2
    var endloop, loopinc;
    var len = message.length;
    var chunk = 0;
    //set up the loops for single and triple des
    var iterations = keys.length == 32 ? 3 : 9; //single or triple des
    if (iterations == 3) {looping = encrypt ? new Array (0, 32, 2) : new Array (30, -2, -2);}
    else {looping = encrypt ? new Array (0, 32, 2, 62, 30, -2, 64, 96, 2) : new Array (94, 62, -2, 32, 64, 2, 30, -2, -2);}

    //pad the message depending on the padding parameter
    if (padding == 2) message += "        "; //pad the message with spaces
    else if (padding == 1) {temp = 8-(len%8); message += String.fromCharCode (temp,temp,temp,temp,temp,temp,temp,temp); if (temp==8) len+=8;} //PKCS7 padding
    else if (!padding) message += "\0\0\0\0\0\0\0\0"; //pad the message out with null bytes

    //store the result here
    result = "";
    tempresult = "";

    if (mode == 1) { //CBC mode
        cbcleft = (iv.charCodeAt(m++) << 24) | (iv.charCodeAt(m++) << 16) | (iv.charCodeAt(m++) << 8) | iv.charCodeAt(m++);
        cbcright = (iv.charCodeAt(m++) << 24) | (iv.charCodeAt(m++) << 16) | (iv.charCodeAt(m++) << 8) | iv.charCodeAt(m++);
        m=0;
    }

    //loop through each 64 bit chunk of the message
    while (m < len) {
        left = (message.charCodeAt(m++) << 24) | (message.charCodeAt(m++) << 16) | (message.charCodeAt(m++) << 8) | message.charCodeAt(m++);
        right = (message.charCodeAt(m++) << 24) | (message.charCodeAt(m++) << 16) | (message.charCodeAt(m++) << 8) | message.charCodeAt(m++);

        //for Cipher Block Chaining mode, xor the message with the previous result
        if (mode == 1) {if (encrypt) {left ^= cbcleft; right ^= cbcright;} else {cbcleft2 = cbcleft; cbcright2 = cbcright; cbcleft = left; cbcright = right;}}

        //first each 64 but chunk of the message must be permuted according to IP
        temp = ((left >>> 4) ^ right) & 0x0f0f0f0f; right ^= temp; left ^= (temp << 4);
        temp = ((left >>> 16) ^ right) & 0x0000ffff; right ^= temp; left ^= (temp << 16);
        temp = ((right >>> 2) ^ left) & 0x33333333; left ^= temp; right ^= (temp << 2);
        temp = ((right >>> 8) ^ left) & 0x00ff00ff; left ^= temp; right ^= (temp << 8);
        temp = ((left >>> 1) ^ right) & 0x55555555; right ^= temp; left ^= (temp << 1);

        left = ((left << 1) | (left >>> 31));
        right = ((right << 1) | (right >>> 31));

        //do this either 1 or 3 times for each chunk of the message
        for (j=0; j<iterations; j+=3) {
            endloop = looping[j+1];
            loopinc = looping[j+2];
            //now go through and perform the encryption or decryption
            for (i=looping[j]; i!=endloop; i+=loopinc) { //for efficiency
                right1 = right ^ keys[i];
                right2 = ((right >>> 4) | (right << 28)) ^ keys[i+1];
                //the result is attained by passing these bytes through the S selection functions
                temp = left;
                left = right;
                right = temp ^ (spfunction2[(right1 >>> 24) & 0x3f] | spfunction4[(right1 >>> 16) & 0x3f]
                    | spfunction6[(right1 >>>  8) & 0x3f] | spfunction8[right1 & 0x3f]
                    | spfunction1[(right2 >>> 24) & 0x3f] | spfunction3[(right2 >>> 16) & 0x3f]
                    | spfunction5[(right2 >>>  8) & 0x3f] | spfunction7[right2 & 0x3f]);
            }
            temp = left; left = right; right = temp; //unreverse left and right
        } //for either 1 or 3 iterations

        //move then each one bit to the right
        left = ((left >>> 1) | (left << 31));
        right = ((right >>> 1) | (right << 31));

        //now perform IP-1, which is IP in the opposite direction
        temp = ((left >>> 1) ^ right) & 0x55555555; right ^= temp; left ^= (temp << 1);
        temp = ((right >>> 8) ^ left) & 0x00ff00ff; left ^= temp; right ^= (temp << 8);
        temp = ((right >>> 2) ^ left) & 0x33333333; left ^= temp; right ^= (temp << 2);
        temp = ((left >>> 16) ^ right) & 0x0000ffff; right ^= temp; left ^= (temp << 16);
        temp = ((left >>> 4) ^ right) & 0x0f0f0f0f; right ^= temp; left ^= (temp << 4);

        //for Cipher Block Chaining mode, xor the message with the previous result
        if (mode == 1) {if (encrypt) {cbcleft = left; cbcright = right;} else {left ^= cbcleft2; right ^= cbcright2;}}
        tempresult += String.fromCharCode ((left>>>24), ((left>>>16) & 0xff), ((left>>>8) & 0xff), (left & 0xff), (right>>>24), ((right>>>16) & 0xff), ((right>>>8) & 0xff), (right & 0xff));

        chunk += 8;
        if (chunk == 512) {result += tempresult; tempresult = ""; chunk = 0;}
    } //for every 8 characters, or 64 bits in the message

    //return the result as an array
    return result + tempresult;
} //end of des



//des_createKeys
//this takes as input a 64 bit key (even though only 56 bits are used)
//as an array of 2 integers, and returns 16 48 bit keys
function des_createKeys (key) {
    //declaring this locally speeds things up a bit
    pc2bytes0  = new Array (0,0x4,0x20000000,0x20000004,0x10000,0x10004,0x20010000,0x20010004,0x200,0x204,0x20000200,0x20000204,0x10200,0x10204,0x20010200,0x20010204);
    pc2bytes1  = new Array (0,0x1,0x100000,0x100001,0x4000000,0x4000001,0x4100000,0x4100001,0x100,0x101,0x100100,0x100101,0x4000100,0x4000101,0x4100100,0x4100101);
    pc2bytes2  = new Array (0,0x8,0x800,0x808,0x1000000,0x1000008,0x1000800,0x1000808,0,0x8,0x800,0x808,0x1000000,0x1000008,0x1000800,0x1000808);
    pc2bytes3  = new Array (0,0x200000,0x8000000,0x8200000,0x2000,0x202000,0x8002000,0x8202000,0x20000,0x220000,0x8020000,0x8220000,0x22000,0x222000,0x8022000,0x8222000);
    pc2bytes4  = new Array (0,0x40000,0x10,0x40010,0,0x40000,0x10,0x40010,0x1000,0x41000,0x1010,0x41010,0x1000,0x41000,0x1010,0x41010);
    pc2bytes5  = new Array (0,0x400,0x20,0x420,0,0x400,0x20,0x420,0x2000000,0x2000400,0x2000020,0x2000420,0x2000000,0x2000400,0x2000020,0x2000420);
    pc2bytes6  = new Array (0,0x10000000,0x80000,0x10080000,0x2,0x10000002,0x80002,0x10080002,0,0x10000000,0x80000,0x10080000,0x2,0x10000002,0x80002,0x10080002);
    pc2bytes7  = new Array (0,0x10000,0x800,0x10800,0x20000000,0x20010000,0x20000800,0x20010800,0x20000,0x30000,0x20800,0x30800,0x20020000,0x20030000,0x20020800,0x20030800);
    pc2bytes8  = new Array (0,0x40000,0,0x40000,0x2,0x40002,0x2,0x40002,0x2000000,0x2040000,0x2000000,0x2040000,0x2000002,0x2040002,0x2000002,0x2040002);
    pc2bytes9  = new Array (0,0x10000000,0x8,0x10000008,0,0x10000000,0x8,0x10000008,0x400,0x10000400,0x408,0x10000408,0x400,0x10000400,0x408,0x10000408);
    pc2bytes10 = new Array (0,0x20,0,0x20,0x100000,0x100020,0x100000,0x100020,0x2000,0x2020,0x2000,0x2020,0x102000,0x102020,0x102000,0x102020);
    pc2bytes11 = new Array (0,0x1000000,0x200,0x1000200,0x200000,0x1200000,0x200200,0x1200200,0x4000000,0x5000000,0x4000200,0x5000200,0x4200000,0x5200000,0x4200200,0x5200200);
    pc2bytes12 = new Array (0,0x1000,0x8000000,0x8001000,0x80000,0x81000,0x8080000,0x8081000,0x10,0x1010,0x8000010,0x8001010,0x80010,0x81010,0x8080010,0x8081010);
    pc2bytes13 = new Array (0,0x4,0x100,0x104,0,0x4,0x100,0x104,0x1,0x5,0x101,0x105,0x1,0x5,0x101,0x105);

    //how many iterations (1 for des, 3 for triple des)
    var iterations = key.length > 8 ? 3 : 1; //changed by Paul 16/6/2007 to use Triple DES for 9+ byte keys
    //stores the return keys
    var keys = new Array (32 * iterations);
    //now define the left shifts which need to be done
    var shifts = new Array (0, 0, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 0);
    //other variables
    var lefttemp, righttemp, m=0, n=0, temp;

    for (var j=0; j<iterations; j++) { //either 1 or 3 iterations
        left = (key.charCodeAt(m++) << 24) | (key.charCodeAt(m++) << 16) | (key.charCodeAt(m++) << 8) | key.charCodeAt(m++);
        right = (key.charCodeAt(m++) << 24) | (key.charCodeAt(m++) << 16) | (key.charCodeAt(m++) << 8) | key.charCodeAt(m++);

        temp = ((left >>> 4) ^ right) & 0x0f0f0f0f; right ^= temp; left ^= (temp << 4);
        temp = ((right >>> -16) ^ left) & 0x0000ffff; left ^= temp; right ^= (temp << -16);
        temp = ((left >>> 2) ^ right) & 0x33333333; right ^= temp; left ^= (temp << 2);
        temp = ((right >>> -16) ^ left) & 0x0000ffff; left ^= temp; right ^= (temp << -16);
        temp = ((left >>> 1) ^ right) & 0x55555555; right ^= temp; left ^= (temp << 1);
        temp = ((right >>> 8) ^ left) & 0x00ff00ff; left ^= temp; right ^= (temp << 8);
        temp = ((left >>> 1) ^ right) & 0x55555555; right ^= temp; left ^= (temp << 1);

        //the right side needs to be shifted and to get the last four bits of the left side
        temp = (left << 8) | ((right >>> 20) & 0x000000f0);
        //left needs to be put upside down
        left = (right << 24) | ((right << 8) & 0xff0000) | ((right >>> 8) & 0xff00) | ((right >>> 24) & 0xf0);
        right = temp;

        //now go through and perform these shifts on the left and right keys
        for (var i=0; i < shifts.length; i++) {
            //shift the keys either one or two bits to the left
            if (shifts[i]) {left = (left << 2) | (left >>> 26); right = (right << 2) | (right >>> 26);}
            else {left = (left << 1) | (left >>> 27); right = (right << 1) | (right >>> 27);}
            left &= -0xf; right &= -0xf;

            //now apply PC-2, in such a way that E is easier when encrypting or decrypting
            //this conversion will look like PC-2 except only the last 6 bits of each byte are used
            //rather than 48 consecutive bits and the order of lines will be according to
            //how the S selection functions will be applied: S2, S4, S6, S8, S1, S3, S5, S7
            lefttemp = pc2bytes0[left >>> 28] | pc2bytes1[(left >>> 24) & 0xf]
                | pc2bytes2[(left >>> 20) & 0xf] | pc2bytes3[(left >>> 16) & 0xf]
                | pc2bytes4[(left >>> 12) & 0xf] | pc2bytes5[(left >>> 8) & 0xf]
                | pc2bytes6[(left >>> 4) & 0xf];
            righttemp = pc2bytes7[right >>> 28] | pc2bytes8[(right >>> 24) & 0xf]
                | pc2bytes9[(right >>> 20) & 0xf] | pc2bytes10[(right >>> 16) & 0xf]
                | pc2bytes11[(right >>> 12) & 0xf] | pc2bytes12[(right >>> 8) & 0xf]
                | pc2bytes13[(right >>> 4) & 0xf];
            temp = ((righttemp >>> 16) ^ lefttemp) & 0x0000ffff;
            keys[n++] = lefttemp ^ temp; keys[n++] = righttemp ^ (temp << 16);
        }
    } //for each iterations
    //return the keys we've created
    return keys;
} //end of des_createKeys