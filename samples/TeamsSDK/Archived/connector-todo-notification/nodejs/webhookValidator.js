'use strict';

const dns = require('dns').promises;
const net = require('net');

/**
 * Validates webhook destinations before they are stored or called, to prevent
 * Server-Side Request Forgery (SSRF, CWE-918). A destination is accepted only when
 * it is an absolute HTTPS URL, its host matches a configured allow-list of trusted
 * Microsoft connector endpoints, and none of the host's resolved IP addresses fall
 * into loopback, private, link-local, or other special-use ranges.
 */

function getAllowedHostSuffixes() {
    const raw = process.env.AllowedWebhookHostSuffixes || '.webhook.office.com,.logic.azure.com';
    return raw
        .split(',')
        .map(s => s.trim())
        .filter(s => s.length > 0);
}

function isHostAllowed(host, allowedHostSuffixes) {
    if (!allowedHostSuffixes || allowedHostSuffixes.length === 0) {
        // Fail closed: without a configured allow-list nothing is trusted.
        return false;
    }

    const lowerHost = host.toLowerCase();
    return allowedHostSuffixes.some(suffix => {
        const normalized = suffix.toLowerCase();
        const bare = normalized.replace(/^\./, '');
        const dotted = normalized.startsWith('.') ? normalized : '.' + normalized;
        return lowerHost === bare || lowerHost.endsWith(dotted);
    });
}

function ipToBytes(ip) {
    return ip.split('.').map(part => parseInt(part, 10));
}

function isPrivateOrReservedV4(ip) {
    const b = ipToBytes(ip);
    if (b.length !== 4 || b.some(n => Number.isNaN(n))) return true;

    if (b[0] === 0) return true;                              // 0.0.0.0/8
    if (b[0] === 10) return true;                             // 10.0.0.0/8
    if (b[0] === 100 && b[1] >= 64 && b[1] <= 127) return true; // 100.64.0.0/10 CGNAT
    if (b[0] === 127) return true;                            // 127.0.0.0/8 loopback
    if (b[0] === 169 && b[1] === 254) return true;            // 169.254.0.0/16 link-local
    if (b[0] === 172 && b[1] >= 16 && b[1] <= 31) return true; // 172.16.0.0/12
    if (b[0] === 192 && b[1] === 168) return true;            // 192.168.0.0/16
    if (b[0] >= 224) return true;                             // multicast + reserved
    return false;
}

function isPrivateOrReservedV6(ip) {
    const lower = ip.toLowerCase();

    // IPv4-mapped IPv6 (::ffff:a.b.c.d) -> validate the embedded IPv4.
    const mapped = lower.match(/^::ffff:(\d+\.\d+\.\d+\.\d+)$/);
    if (mapped) {
        return isPrivateOrReservedV4(mapped[1]);
    }

    if (lower === '::1') return true;   // loopback
    if (lower === '::') return true;    // unspecified

    if (lower.startsWith('fe8') || lower.startsWith('fe9') ||
        lower.startsWith('fea') || lower.startsWith('feb')) {
        return true;                    // fe80::/10 link-local
    }
    if (lower.startsWith('fc') || lower.startsWith('fd')) return true; // fc00::/7 ULA
    if (lower.startsWith('ff')) return true;                           // ff00::/8 multicast

    return false;
}

function isPrivateOrReserved(ip) {
    const family = net.isIP(ip);
    if (family === 4) return isPrivateOrReservedV4(ip);
    if (family === 6) return isPrivateOrReservedV6(ip);
    return true; // Unknown format: treat as unsafe.
}

/**
 * Returns { valid: boolean, reason: string } for the supplied webhook URL.
 */
async function validateWebhook(webhookUrl, allowedHostSuffixes = getAllowedHostSuffixes()) {
    if (!webhookUrl || typeof webhookUrl !== 'string' || webhookUrl.trim().length === 0) {
        return { valid: false, reason: 'Webhook URL is empty.' };
    }

    let uri;
    try {
        uri = new URL(webhookUrl);
    } catch (e) {
        return { valid: false, reason: 'Webhook URL is not a valid absolute URL.' };
    }

    if (uri.protocol !== 'https:') {
        return { valid: false, reason: 'Webhook URL must use the https scheme.' };
    }

    if (uri.username || uri.password) {
        return { valid: false, reason: 'Webhook URL must not contain user information.' };
    }

    const host = uri.hostname;
    if (!isHostAllowed(host, allowedHostSuffixes)) {
        return { valid: false, reason: 'Webhook host is not in the allow-list of trusted connector endpoints.' };
    }

    let addresses;
    try {
        if (net.isIP(host)) {
            addresses = [{ address: host }];
        } else {
            addresses = await dns.lookup(host, { all: true });
        }
    } catch (e) {
        return { valid: false, reason: 'Webhook host could not be resolved.' };
    }

    if (!addresses || addresses.length === 0) {
        return { valid: false, reason: 'Webhook host did not resolve to any address.' };
    }

    for (const entry of addresses) {
        if (isPrivateOrReserved(entry.address)) {
            return { valid: false, reason: 'Webhook host resolves to a private, loopback, or reserved address.' };
        }
    }

    return { valid: true, reason: '' };
}

module.exports = { validateWebhook, getAllowedHostSuffixes };
