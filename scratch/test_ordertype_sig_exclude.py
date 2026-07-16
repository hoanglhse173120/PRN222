import urllib.parse
import hmac
import hashlib
import urllib.request
import urllib.error
import datetime

tmn_code = '3QG8J8RV'
hash_secret = 'XJ3H80JBHO7XX0WQ3HYGDWFN8C6L5T6C'
base_url = 'https://sandbox.vnpayment.vn/paymentv2/vpcpay.html'

current_time = datetime.datetime.now().strftime('%Y%m%d%H%M%S')

params = {
    'vnp_Version': '2.1.0',
    'vnp_Command': 'pay',
    'vnp_TmnCode': tmn_code,
    'vnp_Amount': '5000000',
    'vnp_CreateDate': current_time,
    'vnp_CurrCode': 'VND',
    'vnp_IpAddr': '127.0.0.1',
    'vnp_Locale': 'vn',
    'vnp_OrderInfo': 'ThanhToanGoiCuocStandard',
    'vnp_OrderType': 'other',
    'vnp_ReturnUrl': 'https://localhost:7041/Pricing/VnpayReturn',
    'vnp_TxnRef': '639198169252940983'
}

sorted_keys = sorted(params.keys())

# Exclude vnp_OrderType from signing keys
sig_keys = [k for k in sorted_keys if k != 'vnp_OrderType']

def try_method(name, encode_fn, sign_on_encoded):
    if sign_on_encoded:
        sign_parts = []
        for k in sig_keys:
            sign_parts.append(f"{encode_fn(k)}={encode_fn(params[k])}")
        sign_data = "&".join(sign_parts)
    else:
        sign_parts = []
        for k in sig_keys:
            sign_parts.append(f"{k}={params[k]}")
        sign_data = "&".join(sign_parts)
        
    signature = hmac.new(hash_secret.encode('utf-8'), sign_data.encode('utf-8'), hashlib.sha512).hexdigest()
    
    req_parts = []
    for k in sorted_keys:
        req_parts.append(f"{encode_fn(k)}={encode_fn(params[k])}")
    req_url = base_url + "?" + "&".join(req_parts) + "&vnp_SecureHash=" + signature
    
    try:
        req = urllib.request.Request(req_url, headers={'User-Agent': 'Mozilla/5.0'})
        with urllib.request.urlopen(req) as response:
            final_url = response.geturl()
            content = response.read().decode('utf-8', errors='ignore')
            if 'Error.html' in final_url or 'code=70' in final_url or 'Sai chữ ký' in content:
                print(f"[{name}]: FAILED. Redirected to: {final_url}")
            else:
                print(f"[{name}]: SUCCESS! Final URL: {final_url}")
    except Exception as e:
        print(f"[{name}]: ERROR: {e}")

try_method("Sign Raw (Exclude OrderType in sig)", urllib.parse.quote, sign_on_encoded=False)
try_method("Sign Encoded (Exclude OrderType in sig)", urllib.parse.quote, sign_on_encoded=True)
