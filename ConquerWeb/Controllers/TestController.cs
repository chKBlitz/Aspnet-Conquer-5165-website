// Controllers/TestController.cs
using Microsoft.AspNetCore.Mvc;

[Route("testipn")]
[ApiController]
public class TestController : ControllerBase
{
    [HttpPost]
    public IActionResult ReceiveTest()
    {
        // Bu metodun tetiklenip tetiklenmediğini anlamak için buraya bir breakpoint koyun
        // veya çok basit bir log dosyasına yazın.
        System.IO.File.AppendAllText("test_ipn_log.txt", $"Test IPN alındı: {DateTime.Now}\n");
        return Ok("Test IPN Received");
    }
}