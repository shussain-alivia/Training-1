Issue Summary:
Title: Update System.Linq.Dynamic.Core due to CVE-2023-32571

Issue Description:
Critical vulnerability (CVE-2023-32571) found in System.Linq.Dynamic.Core version 1.2.19 used in FFS Prescriber. Attackers can execute arbitrary code via untrusted inputs. Severity score: 9.8. Versions > 1.0.7.10 and < 1.3.0 affected. Update to 1.3.0 or higher recommended. Detected: Dec 15, 2023 - Last observed: Jan 4, 2024. Fix available since Jun 28th, 2023.

Recommendation:
Urgent update to System.Linq.Dynamic.Core 1.3.0+ advised to mitigate critical security risk from CVE-2023-32571.


Issue Summary:
Title: Upgrade Newtonsoft.Json due to GHSA-5crp-9r3c-p9vr

Issue Description:
Newertonsoft.Json version 12.0.3 in FFS Prescriber is vulnerable to GHSA-5crp-9r3c-p9vr, rated as High severity. Versions below 13.0.1 are affected. The vulnerability allows a Denial of Service (DoS) through improper handling of high-nested expressions, leading to StackOverFlow exceptions or high resource usage. Detection: Dec 15, 2023 - Last seen: Jan 4, 2024. Fix available since Jun 5th, 2023.

Recommendation:
Upgrade Newtonsoft.Json to version 13.0.1 or higher immediately to prevent Denial of Service attacks due to GHSA-5crp-9r3c-p9vr vulnerability.


Issue Summary:
Title: Update Azure.Identity due to CVE-2023-36414

Issue Description:
Azure.Identity version 1.8.0 in FFS Prescriber is vulnerable to CVE-2023-36414 with a High severity rating. Versions < 1.10.2 are affected by a Remote Code Execution vulnerability. The vulnerability enables attackers to execute remote code. Severity Score: 8.8. Detection: Dec 15, 2023 - Last seen: Jan 4, 2024. Fix available since Oct 18th, 2023.

Recommendation:
Immediate update to Azure.Identity 1.10.2+ is crucial to mitigate the Remote Code Execution vulnerability (CVE-2023-36414) and prevent potential attacks.
