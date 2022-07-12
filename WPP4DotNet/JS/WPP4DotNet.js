window.WPP.chatList = async function(filter,label) {
	let list=[];
	switch (filter) {
	  case 'user':
		list = await window.WPP.chat.list({onlyUsers:true});
		break;
	  case 'group':
		list = await window.WPP.chat.list({onlyGroups:true});
		break;
	  case 'label':
		list = await window.WPP.chat.list({withLabels: label});
		break;
	  case 'unread':
		list = await window.WPP.chat.list({onlyWithUnreadMessage:true});
		break;
	  default:
		list = await window.WPP.chat.list();
	}
	let ret = [];
	for (let i = 0; i < list.length; i++) {
		if (list[i]) {
			//let image = await WPP.contact.getProfilePictureUrl(list[i].id.user).then();
			let obj = {
				hasUnread:list[i].hasUnread,
				type:list[i].kind,
				messages:list[i].msgs._models,
				lastMessage:list[i].lastReceivedKey,
				contact:{
					id:list[i].id.user,
					server:list[i].id.server,
					name:list[i].formattedTitle,
					pushname:list[i].contact.pushname,
					isUser:list[i].isUser,
					isGroup:list[i].isGroup,
					isBroadcast:list[i].isBroadcast,
					isMe:list[i].contact.isMe,
					isBusiness:list[i].contact.isBusiness,
					isMyContact:list[i].contact.isMyContact,
					isWAContact:list[i].contact.isWAContact,
					//image:image
					image: ""
				}
			}
			ret.push(obj); 
		}
	}
	return (ret);	
},
window.WPP.chatFind = async function(chat) {
	let item = await window.WPP.chat.find(chat);
	//let image = await WPP.contact.getProfilePictureUrl(item.id.user).then();
	let obj = {
		hasUnread:item.hasUnread,
		type:item.kind,
		messages:item.msgs._models,
		lastMessage:item.lastReceivedKey,
		contact:{
			id:item.id.user,
			server:item.id.server,
			name:item.formattedTitle,
			pushname:item.contact.pushname,
			isUser:item.isUser,
			isGroup:item.isGroup,
			isBroadcast:item.isBroadcast,
			isMe:item.contact.isMe,
			isBusiness:item.contact.isBusiness,
			isMyContact:item.contact.isMyContact,
			isWAContact:item.contact.isWAContact,
			//image:image
			image: ""
		}
	}
	return (obj);	
},
window.WPP.contactList = async function(filter,label) {
	let list=[];
	switch (filter) {
	  case 'my':
		list = await window.WPP.contact.list({onlyMyContacts:true});
		break;
	  case 'label':
		list = await window.WPP.contact.list({withLabels: label});
		break;
	  default:
		list = await window.WPP.contact.list();
	}
	let ret = [];
	for (let i = 0; i < list.length; i++) {
		if (list[i]) {
			let obj = {
				id:list[i].id.user,
				server:list[i].id.server,
				name:list[i].name,
				pushname:list[i].formattedName,
				isUser:list[i].isUser,
				isGroup:list[i].isGroup,
				isBroadcast:list[i].isBroadcast,
				isMe:list[i].isMe,
				isBusiness:list[i].isBusiness,
				isMyContact:list[i].isMyContact,
				isWAContact:list[i].isWAContact,
				//image:image
				image: ""
			}
			ret.push(obj); 
		}
	}
	return (ret);	
}