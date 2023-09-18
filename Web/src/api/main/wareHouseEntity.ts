import request from '/@/utils/request';
enum Api {
  AddWareHouseEntity = '/api/wareHouseEntity/add',
  DeleteWareHouseEntity = '/api/wareHouseEntity/delete',
  UpdateWareHouseEntity = '/api/wareHouseEntity/update',
  PageWareHouseEntity = '/api/wareHouseEntity/page',
}

// 增加warehouseCs
export const addWareHouseEntity = (params?: any) =>
	request({
		url: Api.AddWareHouseEntity,
		method: 'post',
		data: params,
	});

// 删除warehouseCs
export const deleteWareHouseEntity = (params?: any) => 
	request({
			url: Api.DeleteWareHouseEntity,
			method: 'post',
			data: params,
		});

// 编辑warehouseCs
export const updateWareHouseEntity = (params?: any) => 
	request({
			url: Api.UpdateWareHouseEntity,
			method: 'post',
			data: params,
		});

// 分页查询warehouseCs
export const pageWareHouseEntity = (params?: any) => 
	request({
			url: Api.PageWareHouseEntity,
			method: 'post',
			data: params,
		});


