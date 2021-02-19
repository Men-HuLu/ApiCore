# ApiCore
net Core平台Http请求封装接口
支持带3种参数类型
header
body
from-data

Demo:
Request类

    public class DateRequest : IRequest<DateRsponse>
    {
        public DateRequest(string _cardNo, string _patientID) {
            cardNo = _cardNo;
            patientID = _patientID;
        }
        [HttpProperty(HttpMember.From)]
        public string cardNo { get; set; }
        [HttpProperty(HttpMember.From)]
        public string patientID { get; set; }
        public HttpMethod GetMethod() => HttpMethod.POST;
        public string GetPath() => $"rest/getYYDateList";
      
    }

Rsponse类

    public class DateRsponse
    {
        public Obj2 obj { get; set; }
        public string msg { get; set; }
        public bool success { get; set; }
    }

    public class Obj2
    {
        public Appointmentdate[] AppointmentDate { get; set; }
    }
    
    
    请求例子
    private void GetHttpDate()
    {
        var client = new DefaultCoreClient(baseAddress);
        DateRequest req = new DateRequest(yYPatientData.Patient.cardNo, yYPatientData.Patient.patientID);
        var res = client.Execute(req);
        if (res.Success && res.Body.success)
        {
            var appointmentdate = res.Body.obj.AppointmentDate.First(r => r.Code == robDate);
        }
    }
