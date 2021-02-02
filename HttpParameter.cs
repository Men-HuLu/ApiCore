namespace ApiCore
{
	public class HttpParameter
	{
		public HttpMember Member { get; }
		public string Name { get; private set; }
		public object Value { get; }
		public HttpParameter(HttpMember member, string name, object value)
		{
			Member = member;
			Name = name;
			Value = value;
		}
	}
}
