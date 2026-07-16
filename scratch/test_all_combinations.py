import urllib.parse
import hmac
import hashlib
import asyncio
import aiohttp
import datetime

tmn_code = '3QG8J8RV'
hash_secret = 'XJ3H80JBHO7XX0WQ3HYGDWFN8C6L5T6C'
base_url = 'https://sandbox.vnpayment.vn/paymentv2/vpcpay.html'

# Generate combinations
versions = ['2.1.0', '2.0.0']
order_types = ['other', '190000', '250000', 'billpayment']
locales = ['vn', 'en', 'VN']
bank_codes = [None, 'NCB']

now = datetime.datetime.now()
times = [
    now.strftime('%Y%m%d%H%M%S'),
    (now - datetime.timedelta(minutes=5)).strftime('%Y%m%d%H%M%S'),
    (now - datetime.timedelta(minutes=10)).strftime('%Y%m%d%H%M%S'),
    (now + datetime.timedelta(minutes=5)).strftime('%Y%m%d%H%M%S')
]

combinations = []
for ver in versions:
    for ot in order_types:
        for loc in locales:
            for bc in bank_codes:
                for t in times:
                    combinations.append((ver, ot, loc, bc, t))

print(f"Generated {len(combinations)} combinations to test.")

async def test_comb(session, sem, idx, info):
    ver, ot, loc, bc, t = info
    expire_time = (datetime.datetime.strptime(t, '%Y%m%d%H%M%S') + datetime.timedelta(minutes=15)).strftime('%Y%m%d%H%M%S')
    
    params = {
        'vnp_Version': ver,
        'vnp_Command': 'pay',
        'vnp_TmnCode': tmn_code,
        'vnp_Amount': '5000000',
        'vnp_CreateDate': t,
        'vnp_CurrCode': 'VND',
        'vnp_IpAddr': '127.0.0.1',
        'vnp_Locale': loc,
        'vnp_OrderInfo': 'ThanhToanGoiCuocStandard',
        'vnp_ReturnUrl': 'https://localhost:7041/Pricing/VnpayReturn',
        'vnp_TxnRef': '639198169252940983'
    }
    
    if ot is not None:
        params['vnp_OrderType'] = ot
    if bc is not None:
        params['vnp_BankCode'] = bc
    if ver == '2.1.0':
        params['vnp_ExpireDate'] = expire_time
        
    sorted_keys = sorted(params.keys())
    
    # 1. Test Sign Raw
    sign_parts_raw = []
    for k in sorted_keys:
        sign_parts_raw.append(f"{k}={params[k]}")
    sign_data_raw = "&".join(sign_parts_raw)
    sig_raw = hmac.new(hash_secret.encode('utf-8'), sign_data_raw.encode('utf-8'), hashlib.sha512).hexdigest()
    
    req_parts_raw = []
    for k in sorted_keys:
        req_parts_raw.append(f"{urllib.parse.quote(k)}={urllib.parse.quote(params[k])}")
    url_raw = base_url + "?" + "&".join(req_parts_raw) + "&vnp_SecureHash=" + sig_raw

    # 2. Test Sign Encoded
    sign_parts_enc = []
    for k in sorted_keys:
        sign_parts_enc.append(f"{urllib.parse.quote(k)}={urllib.parse.quote(params[k])}")
    sign_data_enc = "&".join(sign_parts_enc)
    sig_enc = hmac.new(hash_secret.encode('utf-8'), sign_data_enc.encode('utf-8'), hashlib.sha512).hexdigest()
    
    url_enc = base_url + "?" + "&".join(req_parts_raw) + "&vnp_SecureHash=" + sig_enc

    async with sem:
        # Check raw
        try:
            async with session.get(url_raw, allow_redirects=True, timeout=10) as response:
                f_url = str(response.url)
                body = await response.text()
                if not ('Error.html' in f_url or 'code=70' in f_url or 'Sai chữ ký' in body):
                    print(f"\n[SUCCESS RAW] version={ver}, ordertype={ot}, locale={loc}, bankcode={bc}, time={t}")
                    print(f"Final URL: {f_url}")
                    return ("raw", ver, ot, loc, bc, t)
        except Exception:
            pass

        # Check encoded
        try:
            async with session.get(url_enc, allow_redirects=True, timeout=10) as response:
                f_url = str(response.url)
                body = await response.text()
                if not ('Error.html' in f_url or 'code=70' in f_url or 'Sai chữ ký' in body):
                    print(f"\n[SUCCESS ENCODED] version={ver}, ordertype={ot}, locale={loc}, bankcode={bc}, time={t}")
                    print(f"Final URL: {f_url}")
                    return ("encoded", ver, ot, loc, bc, t)
        except Exception:
            pass

    return None

async def main():
    sem = asyncio.Semaphore(40)
    async with aiohttp.ClientSession(headers={'User-Agent': 'Mozilla/5.0'}) as session:
        tasks = []
        for idx, info in enumerate(combinations):
            tasks.append(test_comb(session, sem, idx, info))
        
        print("Starting brute force combinations...")
        results = await asyncio.gather(*tasks)
        successful = [r for r in results if r is not None]
        if not successful:
            print("\nAll combinations failed.")
        else:
            print(f"\nDone. Found {len(successful)} successful combinations.")

if __name__ == '__main__':
    asyncio.run(main())
