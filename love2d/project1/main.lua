debug = true
player = { x = 200, y = 710, speed = 150, img = nil }
canShoot = true
canShootTimerMax = 0.2 
canShootTimer = canShootTimerMax
bulletImg = nil
bullets = {}
--引擎加载
function love.load(args) 
	player.img = love.graphics.newImage('assets/plane.png')
	bulletImg = love.graphics.newImage('assets/bullet.png')
end
 --逻辑循环
function love.update(dt) 
	-- I always start with an easy way to exit the game
	if love.keyboard.isDown('escape') then
		love.event.push('quit')
	end
	
	-- player move
	if love.keyboard.isDown('left', 'a') then
		if player.x > 0 then
			player.x = player.x - (player.speed * dt)
		end
	elseif love.keyboard.isDown('right', 'd') then
		if player.x < (love.graphics.getWidth() - player.img:getWidth()) then
			player.x = player.x + (player.speed * dt)
		end
	end
	
	-- Time out how far apart our shots can be.
	if canShootTimer < 0 then
		canShoot = true
	else
		canShootTimer = canShootTimer - dt
	end
	if love.keyboard.isDown('space', 'rctrl', 'lctrl') and canShoot then
		-- Create some bullets
		newBullet = { x = player.x + (player.img:getWidth()/2), y = player.y, img = bulletImg }
		table.insert(bullets, newBullet)
		canShoot = false
		canShootTimer = canShootTimerMax
	end
	
end
 --绘图
function love.draw(dt) 
	love.graphics.draw(player.img, player.x, player.y);
	
	for i, bullet in ipairs(bullets) do
		love.graphics.draw(bullet.img,bullet.x, bullet.y)
	end
end
--键盘交互
function love.keypressed(key) 

end

