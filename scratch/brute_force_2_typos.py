import urllib.parse
import hmac
import hashlib
import asyncio
import aiohttp
import datetime

tmn_code = '3QG8J8RV'
base_url = 'https://sandbox.vnpayment.vn/paymentv2/vpcpay.html'

now = datetime.datetime.now()
current_time = now.strftime('%Y%m%d%H%M%S')
expire_time = (now + datetime.timedelta(minutes=15)).strftime('%Y%m%d%H%M%S')

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
    'vnp_TxnRef': '639198169252940983',
    'vnp_ExpireDate': expire_time
}

sorted_keys = sorted(params.keys())

# Build sign_data (encoded upper)
sign_parts = []
for k in sorted_keys:
    sign_parts.append(f"{urllib.parse.quote(k)}={urllib.parse.quote(params[k])}")
sign_data_enc = "&".join(sign_parts)

# URL query string
req_parts = []
for k in sorted_keys:
    req_parts.append(f"{urllib.parse.quote(k)}={urllib.parse.quote(params[k])}")
req_base = base_url + "?" + "&".join(req_parts) + "&vnp_SecureHash="

original_key = list("XJ3H80JBHO7XX0WQ3HYGDWFN8C6L5T6C")

# Define possible values for ambiguous positions
options = {
    5: ['0', 'O'],
    9: ['O', '0', 'Q'],
    13: ['0', 'O'],
    15: ['Q', 'O', '0'],
    19: ['G', '6', 'C'],
    24: ['8', 'B'],
    26: ['6', 'G'],
    28: ['5', 'S'],
    30: ['6', 'G']
}

# Generate combinations
keys_to_test = []

def generate(idx, current_list):
    if idx == len(options):
        keys_to_test.append("".join(current_list))
        return
    pos = list(options.keys())[idx]
    for val in options[pos]:
        new_list = current_list.copy()
        new_list[pos] = val
        generate(idx + 1, new_list)

generate(0, original_key)

print(f"Generated {len(keys_to_test)} fuzzy key combinations to test.")

async def test_key(session, sem, idx, key):
    # Sign
    sig = hmac.new(key.encode('utf-8'), sign_data_enc.encode('utf-8'), hashlib.sha512).hexdigest()
    url = req_base + sig
    
    async with sem:
        try:
            async with session.get(url, allow_redirects=True, timeout=10) as response:
                final_url = str(response.url)
                body = await response.text()
                if not ('Error.html' in final_url or 'code=70' in final_url or 'Sai chữ ký' in body):
                    print(f"\n[FOUND SUCCESS] Key: {key}")
                    print(f"Redirected to: {final_url}")
                    return key
        except Exception:
            pass
    return None

async def main():
    sem = asyncio.Semaphore(40) # Concurrency limit
    async with aiohttp.ClientSession(headers={'User-Agent': 'Mozilla/5.0'}) as session:
        tasks = []
        for idx, key in enumerate(keys_to_test):
            tasks.append(test_key(session, sem, idx, key))
        
        print("Starting brute force requests...")
        results = await asyncio.gather(*tasks)
        successful_keys = [r for r in results if r is not None]
        if not successful_keys:
            print("\nAll combinations failed. No valid key found.")
        else:
            print(f"\nDone. Found {len(successful_keys)} valid keys.")

if __name__ == '__main__':
    asyncio.run(main())
