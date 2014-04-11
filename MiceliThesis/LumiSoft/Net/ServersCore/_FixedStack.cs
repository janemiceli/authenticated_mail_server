using System;
using System.Collections;

namespace LumiSoft.Net
{
	/// <summary>
	/// Summary description for _FixedStack.
	/// </summary>
	internal class _FixedStack
	{
		private byte[] m_SackList   = null;
		private byte[] m_TerminaTor = null;

		/// <summary>
		/// Terninator holder and checker stack.
		/// </summary>
		/// <param name="terminator"></param>
		public _FixedStack(string terminator)
		{			
		//	m_SackList = new ArrayList();			
			m_TerminaTor = System.Text.Encoding.ASCII.GetBytes(terminator);
			m_SackList = new byte[m_TerminaTor.Length];

			// Init empty array
			for(int i=0;i<m_TerminaTor.Length;i++){
				m_SackList[i] = (byte)0;
			//	m_SackList.Add((byte)0);
			}
		}

		#region function Push

		/// <summary>
		/// Pushes new bytes to stack.(Last in, first out). 
		/// </summary>
		/// <param name="bytes"></param>
		/// <param name="count">Count to push from bytes parameter</param>
		/// <returns>Returns number of bytes may be pushed next push.
		/// NOTE: returns 0 if stack contains terminator.
		/// </returns>
		public int Push(byte[] bytes,int count)
		{
			if(bytes.Length > m_TerminaTor.Length){
				throw new Exception("bytes.Length is too big, can't be more than terminator.length !");
			}
			
			// Move stack bytes which will stay and append new ones
			Array.Copy(m_SackList,count,m_SackList,0,m_SackList.Length - count);
			Array.Copy(bytes,0,m_SackList,m_SackList.Length - count,count);
		
			int index = Array.IndexOf(m_SackList,m_TerminaTor[0]);
			if(index > -1){
				if(index == 0){
					// Check if contains full terminator
					for(int i=0;i<m_SackList.Length;i++){
						if((byte)m_SackList[i] != m_TerminaTor[i]){
							return 1;
						}
					}
					return 0; // If reaches so far, contains terminator
				}
				
				return 1;				
			}
			else{
				return m_TerminaTor.Length;
			}

			// Last in, first out
		//	m_SackList.AddRange(bytes);
		//	m_SackList.RemoveRange(0,bytes.Length);

		//	if(m_SackList.Contains(m_TerminaTor[0])){
		//		return 1;
		//	}
		//	else{
		//		return m_TerminaTor.Length;
		//	}
		}

		#endregion

		#region function ContainsTerminator

		/// <summary>
		/// Check if stack contains terminator.
		/// </summary>
		/// <returns></returns>
		public bool ContainsTerminator()
		{	
			for(int i=0;i<m_SackList.Length;i++){
				if((byte)m_SackList[i] != m_TerminaTor[i]){
					return false;
				}
			}

			return true;
		}

		#endregion


		#region Properties Implementation

	/*	/// <summary>
		/// 
		/// </summary>
		public int Count
		{
			get{ return m_SackList.Count; }
		}*/

		#endregion

	}
}
